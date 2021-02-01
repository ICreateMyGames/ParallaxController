using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour {

    public Movement horizontal;
    public Movement vertical;


    private List<ParallaxImage> images;
    private float timeSinceInit;


    private void Start() {
        InitController();
    }

#if UNITY_EDITOR && false
    public void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            InitController();
        }
    }
#endif


    private void FixedUpdate() {
        timeSinceInit += Time.deltaTime;
        if (images == null) return;
        if (horizontal.type == MoveType.OverTime) MoveOverTimeX();
        else if (horizontal.type == MoveType.FollowTransform) FollowTransformX();
        if (vertical.type == MoveType.OverTime) MoveOverTimeY();
        else if (vertical.type == MoveType.FollowTransform) FollowTransformY();
    }

    private void MoveOverTimeX() {
        if (horizontal.direction == Direction.Fix) return;
        foreach (var item in images) {
            item.MoveX(Time.deltaTime);
        }
    }
    private void MoveOverTimeY() {
        if (vertical.direction == Direction.Fix) return;
        float distance = Mathf.Sin(timeSinceInit * vertical.speedMultiplier) + 1;
        foreach (var item in images) {
            item.SetY(distance);
        }
    }
    private void FollowTransformY() {
        if (vertical.direction == Direction.Fix) return;

        float distance = vertical.lastPos - vertical.transformToFollow.position.y;
        if (Mathf.Abs(distance) < 0.001f) return;
        if (Mathf.Abs(distance) > 5) {
            vertical.lastPos = vertical.transformToFollow.position.y;
            return;
        }
        foreach (var item in images) {
            item.MoveY(distance);
        }
        vertical.lastPos = vertical.transformToFollow.position.y;
    }

    private void FollowTransformX() {
        if (horizontal.direction == Direction.Fix) return;

        float distance = horizontal.lastPos - horizontal.transformToFollow.position.x;
        if (Mathf.Abs(distance) < 0.001f) return;
        if (Mathf.Abs(distance) > 5) {
            horizontal.lastPos = horizontal.transformToFollow.position.x;
            return;
        }
        foreach (var item in images) {
            item.MoveX(distance);
        }

        horizontal.lastPos = horizontal.transformToFollow.position.x;
    }

    public void InitController() {
        timeSinceInit = 0;
        InitList();
        ScanForImages();

        foreach (var item in images) {
            item.InitImage(horizontal, vertical);
        }
        if (horizontal.type == MoveType.FollowTransform) {
            horizontal.lastPos = horizontal.transformToFollow.position.x;
        }
        if (vertical.type == MoveType.FollowTransform) {
            vertical.lastPos = vertical.transformToFollow.position.x;
        }
    }

    private void InitList() {
        if (images == null) images = new List<ParallaxImage>();
        else {
            foreach (var item in images) {
                item.CleanUpImage();
            }
            images.Clear();
        }
    }

    private void ScanForImages() {
        ParallaxImage pi;

        foreach (Transform child in transform) {
            if (child.gameObject.activeSelf) {
                pi = child.GetComponent<ParallaxImage>();
                if (pi != null) images.Add(pi);
            }
        }
    }

}

[System.Serializable]
public class Movement {

    public bool randomizeStart;
    [Range(0.01f, 5)]
    public float speedMultiplier = 1;
    public MoveType type;
    public Direction direction;
    public Transform transformToFollow;
    [System.NonSerialized] public float lastPos;
}

public enum Direction {
    Fix,
    Positive,
    Negative
}

 
public enum MoveType {
    OverTime,
    FollowTransform
}
