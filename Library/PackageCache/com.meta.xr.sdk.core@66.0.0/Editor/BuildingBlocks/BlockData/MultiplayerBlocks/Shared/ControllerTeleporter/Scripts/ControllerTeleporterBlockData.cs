/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Linq;
using Meta.XR.BuildingBlocks.Editor;
using UnityEngine;
using Meta.XR.Locomotion.Teleporter;

namespace Meta.XR.MultiplayerBlocks.Shared.Editor
{
    public class ControllerTeleporterBlockData : BlockData
    {
        protected override List<GameObject> InstallRoutine(GameObject selectedGameObject)
        {
            var controllerTeleporterGO = base.InstallRoutine(selectedGameObject)[0];
            var cameraRigBB = Utils.GetBlocksWithType<OVRCameraRig>().First();

            if (cameraRigBB == null)
            {
                Debug.LogWarning("OVRCameraRig block cannot be found, aborting installing Controller Teleporter block");
                return new List<GameObject>();
            }

            var cameraRig = cameraRigBB.GetComponent<OVRCameraRig>();
            controllerTeleporterGO.GetComponent<Teleporter>().ovrCameraRig = cameraRig;

            return new List<GameObject> { controllerTeleporterGO };
        }
    }
}
