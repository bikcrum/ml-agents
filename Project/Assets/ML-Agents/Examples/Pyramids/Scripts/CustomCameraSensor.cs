using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CustomCameraSensor : CameraSensor {
    public CustomCameraSensor(Camera camera, int width, int height, bool grayscale, string name,
        SensorCompressionType compression, ObservationType observationType = ObservationType.Default) : base(camera,
        width, height, grayscale, name, compression, observationType) {
        throw new System.NotImplementedException();
    }
}
