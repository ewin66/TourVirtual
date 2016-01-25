﻿/* Shooter.cs
  version 1.0 - August 7, 2015

  Copyright (C) 2015 Wasabi Applications Inc.
   http://www.wasabi-applications.co.jp/
*/

using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour {

	public Camera cameraForShooter;
	public GameObject ballPrefab;
	public Transform shotPoint;
    public Transform hoopTransform;

    public float targetZ = 12.0f;			//screen point z to world point
	public float shotPowerMin = 3.0f;		//minimum shot power
	public float shotPowerMax = 12.0f;		//maximum shot power
	public float offsetY = 100.0f;			//offset Y for trajectory
	public float shotTimeMin = 0.2f;		//minimum time till to release finger
	public float shotTimeMax = 0.55f;		//maximum time till to release finger
	public float torque = 30.0f;			//torque (backspin)

	public float offsetZShotPoint = 1.0f;	//for rolling ball
	public float powerToRoll = 2.0f;		//for rolling ball
	public float timeoutForShot = 5.0f;		//for error handling

    public float minRad = 3.6f;
    public float maxRad = 5.6f;
    public float minAngle = -45.0f;
    public float maxAngle = 45.0f;

    public float gameTime = 30.0f;
    public int round = 0;
    public int streak = 0;
    public int score = 0;

    // for demo
    public float shotPower {get; private set;}		//shot power (initial velocity)
	public Vector3 direction {get; private set;}	//shot direction (normalized)

	GameObject objBall;
	Rigidbody ballRigidbody;
	float startTime;

	Vector2 touchPos;
    enum GameState
    {
        WaitStart,
        Playing,
        Finished
    }
    GameState gameState = GameState.WaitStart;
    enum ShotState {
        Waiting,
		Charging,					//before shot (rolling)
		Ready,						//ready
		DirectionAndPower			//on swiping
	}
	
	ShotState state = ShotState.Charging;

	// Use this for initialization
	void Start () {
		touchPos.x = -1.0f;
        changePosition();
    }

    public void reset() {
        round = 0;
        streak = 0;
        score = 0;
        gameState = GameState.WaitStart;
        state = ShotState.Charging;
    }

    public void OnScore()
    {
        score++;
        streak++;

        if (objBall != null)
        {
            Destroy(objBall);
            objBall = null;
        }
        state = ShotState.Charging;
        changePosition();
    }

    void OnResetStreak() {
        streak = 0;

    }

    public void changePosition()
    {

        float rad = -Random.Range(minRad, maxRad);
        float ang = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;

        float cos = Mathf.Cos(ang);
        float sin = Mathf.Sin(ang);
        // float tx = x cos - y sin  
        // float tx = x sin + y cos  
        transform.position = new Vector3(- sin * rad, 1.85f, cos * rad );
        transform.LookAt(new Vector3(0,1.85f,0));


    }
   

    // Update is called once per frame
    void Update () {
        switch (gameState)
        {
            case GameState.WaitStart:
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        gameState = GameState.Playing;
                    }
                }
                break;
            case GameState.Playing:
                {
                    gameTime -= Time.deltaTime;
                    if (gameTime < 0)
                    {
                        OnFinishGame();
                    }
                    else {
                        if (state == ShotState.Charging)
                        {
                            ChargeBall();
                            CheckTrigger();

                        }
                        else if (state == ShotState.Ready)
                        {
                            CheckTrigger();

                        }
                        else if (state == ShotState.DirectionAndPower)
                        {
                            CheckShot();
                        }
                    }
                }
                break;
            case GameState.Finished:
                {
                }
                break;
        }
    }

    void FixedUpdate () {
        if (gameState == GameState.Playing)
        {
            if (state != ShotState.Charging && state != ShotState.Waiting)
            {
                ballRigidbody.velocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
            }
            else {
                if (ballRigidbody != null && ballRigidbody.velocity.y < 0 && ballRigidbody.position.y < 1.8f)
                {
                    if (ballRigidbody.name != "goal")
                        OnResetStreak();
                    OnChargeBall();
                }
            }
        }
	}
	
	void ChargeBall () {
		if (objBall == null) {
			objBall = (GameObject)Instantiate(ballPrefab);
			objBall.AddComponent<ShotBall>();
			ballRigidbody = objBall.GetComponent<Rigidbody>();
						
			Vector3 shotPos = shotPoint.transform.localPosition;
			shotPos.z -= offsetZShotPoint;
			objBall.transform.position = shotPoint.transform.TransformPoint(shotPos);
			objBall.transform.eulerAngles = shotPoint.transform.eulerAngles;
            objBall.transform.parent = transform;

            ballRigidbody.velocity = Vector3.zero;

			ballRigidbody.AddForce(shotPoint.transform.TransformDirection(new Vector3 (0.0f, 0.0f, powerToRoll)), ForceMode.Impulse);
		}
		

		
		float dis = Vector3.Distance(shotPoint.transform.position, objBall.transform.position);
		if (dis <= 0.1f) {
			state = ShotState.Ready;
		}
	}

	void CheckTrigger () {
		if (touchPos.x < 0) {
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = cameraForShooter.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 100)) {
					ShotBall sb = hit.collider.transform.GetComponent<ShotBall>();
					if (sb != null && !sb.isActive) {
						sb.ChangeActive();
						touchPos = Input.mousePosition;
						shotPower = 0.0f;
					}
				}
			}
		} else {
			if (touchPos.x != Input.mousePosition.x || touchPos.y != Input.mousePosition.y) {
				touchPos.x = -1.0f;
				startTime = Time.time;
				state = ShotState.DirectionAndPower;
			}
		}
	}

	void CheckShot () {
		float elapseTime = Time.time - startTime;
		
		if (Input.GetMouseButtonUp (0)) {
			if (objBall != null) {
                ShootBall(elapseTime);
			}
			//Invoke("OnChargeBall",1);
            state = ShotState.Waiting;
            objBall = null;
		}
        /*
		if (timeoutForShot < elapseTime) {
			Destroy (objBall);
			state = ShotState.Charging;
			objBall = null;
		}
        */
	}

    void OnChargeBall()
    {
        state = ShotState.Charging;
    }


    void ShootBall (float elapseTime) {
		
		if (elapseTime < shotTimeMin) {
			shotPower = shotPowerMax;
		} else if (shotTimeMax < elapseTime) {
			shotPower = shotPowerMin;
		} else {
			float tmin100 = shotTimeMin * 10000.0f;
			float tmax100 = shotTimeMax * 10000.0f;
			float ep100 = elapseTime * 10000.0f;
			float rate = (ep100 - tmin100) / (tmax100 - tmin100);
			shotPower = shotPowerMax - ((shotPowerMax - shotPowerMin) * rate);
		}

		Vector3 screenPoint = Input.mousePosition;
		screenPoint.z = targetZ;
		Vector3 worldPoint = cameraForShooter.ScreenToWorldPoint (screenPoint);

		worldPoint.y += (offsetY / shotPower);

		direction = (worldPoint - shotPoint.transform.position).normalized;
		
		ballRigidbody.velocity = direction * shotPower;
		ballRigidbody.AddTorque (-shotPoint.transform.right * torque);
        ballRigidbody.transform.parent = transform;

        round++;

        Destroy(ballRigidbody.gameObject, 10);
    }
    void OnFinishGame()
    {
        gameTime = 0;
        gameState = GameState.Finished;
        Destroy(objBall);
        state = ShotState.Waiting;
        objBall = null;
    }

    void OnGUI() {
        GUILayout.Label("State " + gameState);
        GUILayout.Label("Time " + Mathf.CeilToInt(gameTime));
        GUILayout.Label("Round " + round);
        GUILayout.Label("Score " + score);
        GUILayout.Label("Streak " + streak);

    }

}
