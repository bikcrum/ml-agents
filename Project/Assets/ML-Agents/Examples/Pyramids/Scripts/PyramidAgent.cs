using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PyramidAgent : Agent {
    // public GameObject area;

    // PyramidArea m_MyArea;
    Rigidbody m_AgentRb;

    // PyramidSwitch m_SwitchLogic;
    // public GameObject areaSwitch;
    public bool useVectorObs;

    [SerializeField] private Transform goal;

    [SerializeField] private List<Transform> obstaclesTransform;

    [SerializeField] private bool randomizeObstacleRotation;

    private HashSet<(int, int)> visitedState = new HashSet<(int, int)>();

    private float prevTime;

    public override void Initialize() {
        m_AgentRb = GetComponent<Rigidbody>();
        // m_MyArea = area.GetComponent<PyramidArea>();
        // m_SwitchLogic = areaSwitch.GetComponent<PyramidSwitch>();
    }

    public override void CollectObservations(VectorSensor sensor) {
        if (useVectorObs) {
            // sensor.AddObservation(m_SwitchLogic.GetState());
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        }
    }

    public void MoveAgent(ActionSegment<int> act) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action) {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) {
        // Check every 0.5 second if agent is at same place, give reward of -1 else if it discovered new place give reward of 1
        if (Time.time - prevTime > 0.5f) {
            Vector3 position = transform.localPosition;
            int gridX = (int)((position.x + 50) / 10f);
            int gridZ = (int)((position.z + 50) / 10f);

            int gridIndex = gridX * 10 + gridZ;

            int rotationIndex = (int)(transform.eulerAngles.y / 22.5f);

            if (visitedState.Contains((gridIndex, rotationIndex))) {
                AddReward(-1f);
                Debug.Log("Visiting same state");
            }
            else {
                visitedState.Add((gridIndex, rotationIndex));
                Debug.Log("Visiting new state");
                AddReward(1f);
            }

            prevTime = Time.time;
        }

        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D)) {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W)) {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A)) {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S)) {
            discreteActionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin() {
        // var enumerable = Enumerable.Range(0, 9).OrderBy(x => Guid.NewGuid()).Take(9);
        // var items = enumerable.ToArray();

        // m_MyArea.CleanPyramidArea();

        m_AgentRb.velocity = Vector3.zero;
        // m_MyArea.PlaceObject(gameObject, items[0]);
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        // m_SwitchLogic.ResetSwitch(items[1], items[2]);

        goal.localPosition = new Vector3(Random.Range(-45f, 45f), 1f, Random.Range(-45f, 45f));
        goal.Rotate(0f, Random.Range(0f, 180f), 0f);

        if (randomizeObstacleRotation) {
            foreach (var obstacleTransform in obstaclesTransform) {
                obstacleTransform.Rotate(0f, Random.Range(0f, 180f), 0f);
            }
        }
        // m_MyArea.CreateStonePyramid(1, items[3]);
        // m_MyArea.CreateStonePyramid(1, items[4]);
        // m_MyArea.CreateStonePyramid(1, items[5]);
        // m_MyArea.CreateStonePyramid(1, items[6]);
        // m_MyArea.CreateStonePyramid(1, items[7]);
        // m_MyArea.CreateStonePyramid(1, items[8]);
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("goal")) {
            SetReward(2f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Obstacle")) {
            SetReward(-1f);
            transform.localPosition = new Vector3(Random.Range(-45f, 45f), 0f, Random.Range(-45f, 45f));
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("wall")) {
            SetReward(-1f);
            transform.localPosition = new Vector3(Random.Range(-45f, 45f), 0f, Random.Range(-45f, 45f));
            EndEpisode();
        }
    }
}
