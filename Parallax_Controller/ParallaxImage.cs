using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxImage : MonoBehaviour {

    //public 
    public float speedX = 0;
    public float speedY = 0;
    public int spawnCount = 2;
    public float repositionBuffer = .5f;

    public ImageType imageType;
    public InstanceModifier instanceModifier;

    //private
    private const int roundFactor = 1000;

    private Vector3 startPos;
    private float imageWidth;
    private float minLeftX;
    private float maxRightX;
    private Transform[] controlledTransforms;
    private SpriteRenderer sr;


    private Movement vertical;
    private Movement horizontal;


    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    public void RandomizeStartX() {
        MoveX(Random.Range(0, imageWidth), false);
    }
    public void RandomizeStartY() {
        MoveY(Random.Range(0, 1));
    }

    public void MoveY(float moveBy) {
        moveBy *= speedY * vertical.speedMultiplier;
        if (vertical.direction == Direction.Negative) moveBy *= -1;

        for (int i = 0; i < controlledTransforms.Length; i++) {
            Vector3 newPos = controlledTransforms[i].position;
            newPos.y += moveBy;
            controlledTransforms[i].position = newPos;
        }
    }

    public void SetY(float y) {
        y *= speedY * vertical.speedMultiplier;
        if (vertical.direction == Direction.Negative) y *= -1;

        for (int i = 0; i < controlledTransforms.Length; i++) {
            Vector3 newPos = controlledTransforms[i].position;
            newPos.y = y;
            controlledTransforms[i].position = newPos;
        }
    }

    public void MoveX(float moveBy, bool doSpeedCalc = true) {
        if (doSpeedCalc) moveBy *= speedX * horizontal.speedMultiplier;
        if (horizontal.direction == Direction.Negative) moveBy *= -1;

        moveBy = Mathf.Round(moveBy * roundFactor) / roundFactor;
        for (int i = 0; i < controlledTransforms.Length; i++) {
            Vector3 newPos = controlledTransforms[i].position;
            newPos.x += moveBy;
            newPos.x = Mathf.Round(newPos.x * roundFactor) / roundFactor;
            controlledTransforms[i].position = newPos;
        }
        CheckAndReposition();
    }

    private void CheckAndReposition() {
        if (horizontal.direction == Direction.Negative || horizontal.type == MoveType.FollowTransform) {
            for (int i = 0; i < controlledTransforms.Length; i++) {
                if (controlledTransforms[i].position.x < minLeftX) {
                    Vector3 newPos = controlledTransforms[i].position;
                    newPos.x = GetRightmostTransform().position.x + imageWidth;
                    controlledTransforms[i].position = newPos;
                }
            }
        }
        if (horizontal.direction == Direction.Positive || horizontal.type == MoveType.FollowTransform) {
            for (int i = 0; i < controlledTransforms.Length; i++) {
                if (controlledTransforms[i].position.x > maxRightX) {
                    Vector3 newPos = controlledTransforms[i].position;
                    newPos.x = GetLeftmostTransform().position.x - imageWidth;
                    controlledTransforms[i].position = newPos;
                }
            }
        }

    }


    public void CleanUpImage() {
        if (controlledTransforms != null) {
            for (int i = 1; i < controlledTransforms.Length; i++) {
                Destroy(controlledTransforms[i].gameObject);
            }
        }
    }

    public void InitImage(Movement horizontal, Movement vertical) {
        this.horizontal = horizontal;
        this.vertical = vertical;

        transform.position = startPos;

        PrepareVariables();

        CreateImageInstances();

        if (horizontal.randomizeStart) RandomizeStartX();
        if (vertical.randomizeStart) RandomizeStartY();
    }

    private void PrepareVariables() {
        imageWidth = sr.bounds.size.x;
        if (imageType == ImageType.Instance) {
            imageWidth += instanceModifier.spawnBuffer;
        }
        if (horizontal.type == MoveType.FollowTransform) {
            minLeftX = transform.position.x - imageWidth * (spawnCount + 1) - repositionBuffer;
            maxRightX = transform.position.x + imageWidth * (spawnCount + 1) + repositionBuffer;

        } else {
            if (horizontal.direction == Direction.Negative) {
                minLeftX = transform.position.x - imageWidth - repositionBuffer;
                maxRightX = float.PositiveInfinity;
            } else if (horizontal.direction == Direction.Positive) {
                maxRightX = transform.position.x + imageWidth + repositionBuffer;
                minLeftX = float.NegativeInfinity;
            } else if (horizontal.direction == Direction.Fix) {
                minLeftX = float.NegativeInfinity;
                maxRightX = float.PositiveInfinity;
            }
        }
    }

    private void CreateImageInstances() {
        int arraySize = spawnCount;
        if (horizontal.type == MoveType.FollowTransform) arraySize *= 2;
        arraySize += 1;

        controlledTransforms = new Transform[arraySize];
        controlledTransforms[0] = transform;

        float changeBy;
        for (int i = 1; i <= spawnCount; i++) {
            if (horizontal.direction == Direction.Positive) {
                changeBy = -imageWidth * i;
            } else {
                changeBy = imageWidth * i;
            }
            if (imageType == ImageType.Instance && instanceModifier.spawnRandom) {
                changeBy += Random.Range(-instanceModifier.spawnRandomRange, instanceModifier.spawnRandomRange);
            }

            controlledTransforms[i] = PrepareCopyAt(transform.position.x + changeBy);
            if (horizontal.type == MoveType.FollowTransform) controlledTransforms[i + spawnCount] = PrepareCopyAt(transform.position.x - changeBy);
        }

    }

    private Transform PrepareCopyAt(float posX) {
        float posY = transform.position.y;
        Vector3 localScale = transform.localScale;

        if (imageType == ImageType.Instance && instanceModifier.scaleRandom) {
            localScale.x = Random.Range(1, instanceModifier.scaleRandomRange);
            if (Random.value < .5f) localScale.x = 1 / localScale.x;
            localScale.y = localScale.x;

            if (instanceModifier.freezeYBottom) {
                posY = transform.position.y - (sr.bounds.size.y / 2) * ((transform.localScale.y - localScale.y) / transform.localScale.y);
            }
        }

        GameObject go = Instantiate(gameObject, new Vector3(posX, posY, transform.position.z), Quaternion.identity, transform.parent);
        Destroy(go.GetComponent<ParallaxImage>());
        go.transform.localScale = localScale;

        return go.transform;

    }

    private Transform GetRightmostTransform() {
        float currentMaxX = float.NegativeInfinity;
        Transform currentTransform = null;

        for (int i = 0; i < controlledTransforms.Length; i++) {
            if (currentMaxX < controlledTransforms[i].position.x) {
                currentMaxX = controlledTransforms[i].position.x;
                currentTransform = controlledTransforms[i];
            }
        }

        return currentTransform;
    }
    private Transform GetLeftmostTransform() {
        float currentMinX = float.PositiveInfinity;
        Transform currentTransform = null;

        for (int i = 0; i < controlledTransforms.Length; i++) {
            if (currentMinX > controlledTransforms[i].position.x) {
                currentMinX = controlledTransforms[i].position.x;
                currentTransform = controlledTransforms[i];
            }
        }

        return currentTransform;
    }

}

public enum ImageType {
    Seamless,
    Instance
}


[System.Serializable]
public class InstanceModifier {
    [Header("Only for ImageType Instances")]
    public float spawnBuffer = 0;

    public bool spawnRandom = false;
    [Range(0, 5)]
    public float spawnRandomRange = 0;

    [Space]

    public bool scaleRandom = false;
    public bool freezeYBottom = true;
    [Range(1, 4)]
    public float scaleRandomRange = 1;
}