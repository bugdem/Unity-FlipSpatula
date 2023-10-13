using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClocknestGames.Library.Utils;

namespace ClocknestGames.Game.Core
{
    public class CameraManager : Singleton<CameraManager>
    {
        public Camera MainCamera;
        public Camera MinimapCamera;
        public CameraFollow MinimapCameraFollow;
        public CameraSmoothFollow MainCameraFollow;
    }
}