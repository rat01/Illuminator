﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {
	private static Game m_staticRef = null;
	public static Game staticRef {
		get { return m_staticRef; }
	}

	[SerializeField]
	private PlaneManager m_planeManager;

	[SerializeField]
	private GameObject loseScreen;

	public PlaneManager planeManager {
		get { return m_planeManager; }
	}

	private PlayerCharacter m_player;
	public PlayerCharacter player {
		get { return m_player; }
	}

	[SerializeField]
	private ScoreCounter m_scoreCounter;
	public ScoreCounter scoreCounter {
		get { return m_scoreCounter; }
	}

	[SerializeField]
	private BlockPool m_blockPool;
	public BlockPool blockPool {
		get { return m_blockPool; }
	}

	[SerializeField]
	private BlockSpawner m_spawner;
	public BlockSpawner spawner {
		get { return m_spawner; }
	}

	[SerializeField]
	private Boundaries m_boundaries;
	public Boundaries boundaries {
		get { return m_boundaries; }
	}

	private CamShake m_camShake;
	public CamShake camShake {
		get { return m_camShake; }
	}

	private Palette m_palette;
	/// <summary>
	/// Colors of various objects in the game.
	/// </summary>
	/// <returns></returns>
	public Palette palette {
		get { return m_palette; }
	}

	private int difficultyCounter = 0;

	[SerializeField]
	private int difficultyIntervals;

	/// <summary>
	/// Difficulty from zero to one. Increases over time.
	/// </summary>
	public float difficulty {
		get {
			if (difficultyCounter == int.MaxValue) {
				return 1f;
			}
			else {
				return difficultyCounter * 1f / difficultyIntervals;
			}
		}
	}

	private float difficultyTimeElapsed = 0f;
	[SerializeField]
	private float difficultyChangeTime = 5f;

	[SerializeField]
	private AutoScroller m_autoScroller;
	public AutoScroller autoScroller {
		get { return m_autoScroller; }
	}

	void Awake () {
		m_camShake = Camera.main.GetComponent<CamShake> ();
		m_palette = new ProceduralPalette (Random.value.Normalized01 (), Random.Range (1f / 8f, 0.25f));
		m_staticRef = this;
	}

	void OnDestroy () {
		m_staticRef = null;
	}

	
	public bool isMainMenu = false;
	void Start () {
		if (!isMainMenu) {
			m_player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerCharacter> ();
		}
	}

	void Update () {
		if (difficultyCounter < difficultyIntervals) {
			difficultyTimeElapsed += Time.deltaTime;
			if (difficultyTimeElapsed >= difficultyChangeTime) {
				difficultyTimeElapsed = 0f;
				difficultyCounter++;
				m_autoScroller.SetSpeedRatio (difficulty);
				SoundCatalog.staticRef.PlaySpeedUpSound ();
			}
		}
	}

	private static float HALT_DURATION {
		get { return 1.5f; }
	}
	private static InterpolationMethod HALT_INTERP_METHOD {
		get { return InterpolationMethod.Sinusoidal; }
	}
	/// <summary>
	/// Gradually halt the level auto scroll.
	/// </summary>
	public IEnumerator Halt () {
		difficultyCounter = int.MaxValue;
		MusicMaster.staticRef.HaltMusic (HALT_DURATION, HALT_INTERP_METHOD);
		Transform cam = Camera.main.transform.parent.transform;
		float timeElapsed = 0f;
		float originalScrollRate = m_autoScroller.scrollSpeed;
		while (timeElapsed <= HALT_DURATION) {
			timeElapsed += Time.deltaTime;
			float ratio = timeElapsed / HALT_DURATION;
			m_autoScroller.SetSpeedManual (Interpolation.Interpolate (originalScrollRate, 2.5f, ratio, HALT_INTERP_METHOD));
			yield return null;
		}

		loseScreen.SetActive (true);
#if UNITY_EDITOR
		while (!Input.GetButtonDown ("Swap")) {
			yield return null;
		}
#else
	while (!Utility.ScreenTappedThisFrame ()) {
			yield return null;
		}
#endif
		MusicMaster.staticRef.FadeInMusic (1.25f, HALT_INTERP_METHOD);
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);

	}
}
