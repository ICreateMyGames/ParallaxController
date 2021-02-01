using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ParallaxController))]
public class ParallaxControllerEditor : Editor {

    private const int roundFactor = 1000;
    private int startLayer = 1;
    private float startValue = 1;
    private int layerIncrease = 1;
    private bool onlyActive = true;
    private bool speedX = true;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        GUILayout.Space(75);

        if (!Application.isPlaying) {

            GUILayout.Label("Setup Helper Sprite Renderer", EditorStyles.boldLabel);

            onlyActive = EditorGUILayout.Toggle("Only active GameObjects", onlyActive);
            startLayer = EditorGUILayout.IntField("Start Sorting Layer", startLayer);
            layerIncrease = EditorGUILayout.IntField("Layer increase", layerIncrease);

            if (GUILayout.Button("Set SpriteRenderer Layer by Child Index")) {
                //add everthing the button would do.
                Transform t = ((ParallaxController)target).transform;
                int c = startLayer;
                foreach (Transform child in t) {
                    if ((onlyActive && child.gameObject.activeSelf) || !onlyActive) {
                        SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                        if (sr != null) {
                            sr.sortingOrder = c;
                            c += layerIncrease;
                        }
                    }
                }
            }

            GUILayout.Space(20);
            GUILayout.Label("Setup Helper Parallax Image", EditorStyles.boldLabel);


            GUILayout.Space(20);
            GUILayout.Label("Speed", EditorStyles.boldLabel);

            speedX = EditorGUILayout.Toggle("Change SpeedX ( false = SpeedY)", speedX);
            onlyActive = EditorGUILayout.Toggle("Only active GameObjects", onlyActive);
            startValue = EditorGUILayout.FloatField("Start value", startValue);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Inc by 20%")) {
                ChangeSpeed(speedX, startValue, v => v * 1.2f);
            }
            if (GUILayout.Button("Inc by 50%")) {
                ChangeSpeed(speedX, startValue, v => v * 1.5f);
            }
            if (GUILayout.Button("Inc by 100%")) {
                ChangeSpeed(speedX, startValue, v => v * 2f);
            }
            if (GUILayout.Button("Inc by .2f")) {
                ChangeSpeed(speedX, startValue, v => v + .2f);
            }
            if (GUILayout.Button("Inc by .5f")) {
                ChangeSpeed(speedX, startValue, v => v + .5f);
            }
            if (GUILayout.Button("Inc by 1f")) {
                ChangeSpeed(speedX, startValue, v => v + 1f);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dec by 20%")) {
                ChangeSpeed(speedX, startValue, v => v * .8f);
            }
            if (GUILayout.Button("Dec by 50%")) {
                ChangeSpeed(speedX, startValue, v => v * .5f);
            }
            if (GUILayout.Button("Dec by 75%")) {
                ChangeSpeed(speedX, startValue, v => v * .75f);
            }
            if (GUILayout.Button("Dec by .2f")) {
                ChangeSpeed(speedX, startValue, v => v - .2f);
            }
            if (GUILayout.Button("Dec by .5f")) {
                ChangeSpeed(speedX, startValue, v => v - .5f);
            }
            if (GUILayout.Button("Dec by 1f")) {
                ChangeSpeed(speedX, startValue, v => v - 1f);
            }
            GUILayout.EndHorizontal();

        } else {
            if (GUILayout.Button("Reinit Controller")) {
                ((ParallaxController)target).InitController();
            }
        }
    }


    public void ChangeSpeed(bool speedX, float value, Func<float, float> incFunc) {
        Transform t = ((ParallaxController)target).transform;
        foreach (Transform child in t) {
            if ((onlyActive && child.gameObject.activeSelf) || !onlyActive) {
                ParallaxImage pi = child.GetComponent<ParallaxImage>();
                if (pi != null) {
                    if (speedX) pi.speedX = Mathf.Round(value * roundFactor) / roundFactor;
                    else pi.speedY = Mathf.Round(value * roundFactor) / roundFactor;
                    value = incFunc.Invoke(value);
                }
            }
        }
    }





}
