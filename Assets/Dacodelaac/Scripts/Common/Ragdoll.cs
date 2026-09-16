// #define RAGDOLL
#if RAGDOLL
using System;
using System.Collections;
using System.Linq;
using BzKovSoft.CharacterSlicer;
using BzKovSoft.ObjectSlicer;
using Dacodelaac.DebugUtils;
using Dacodelaac.Utils;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Common
{
    public class Ragdoll : BzSliceableBase
    {
        [SerializeField] float totalWeight = 10;
        [SerializeField] Transform hip;
        [SerializeField] Transform spine;
        [SerializeField] Transform chest;
        [SerializeField] Transform head;
        [SerializeField] Transform upperArmL;
        [SerializeField] Transform lowerArmL;
        [SerializeField] Transform handL;
        [SerializeField] Transform upperArmR;
        [SerializeField] Transform lowerArmR;
        [SerializeField] Transform handR;
        [SerializeField] Transform upperLegL;
        [SerializeField] Transform lowerLegL;
        [SerializeField] Transform footL;
        [SerializeField] Transform upperLegR;
        [SerializeField] Transform lowerLegR;
        [SerializeField] Transform footR;

        [SerializeField] Rigidbody[] rigidbodies;
        [SerializeField] Collider[] colliders;
        
        RagdollAttachment[] _ragdollAttachments;
        RagdollAttachment[] ragdollAttachments => this.GetAndCacheComponentsInChildren(ref _ragdollAttachments);

        public Transform Hip => hip;
        Vector3 force;
        bool disable;

        public void Initialize()
        {
            foreach (var c0 in colliders)
            {
                foreach (var c1 in colliders)
                {
                    if (c0 != c1)
                    {
                        Physics.IgnoreCollision(c0, c1);
                    }
                }
            }

            foreach (var attachment in ragdollAttachments)
            {
                attachment.Initialize();
            }
        }

        public void Activate(Vector3 force, bool disable)
        {
            this.force = force;
            this.disable = disable;
            
            foreach (var attachment in ragdollAttachments)
            {
                attachment.Activate(force, disable);
            }
            foreach (var c in colliders)
            {
                c.enabled = true;
            }
            foreach (var r in rigidbodies)
            {
                r.isKinematic = false;
                r.drag = 1f;
                r.angularDrag = 1;
                if (!r.GetComponent<Collider>())
                {
                    r.useGravity = false;
                }
            }
            var chestRigid = chest.GetComponent<Rigidbody>();
            chestRigid.AddForce(force, ForceMode.Impulse);

            if (disable)
            {
                StartCoroutine(IEDisable());
            }
        }

        public void Deactivate()
        {
            foreach (var r in rigidbodies)
            {
                r.isKinematic = true;
            }
            foreach (var c in colliders)
            {
                c.enabled = false;
            }
            foreach (var attachment in ragdollAttachments)
            {
                attachment.Deactivate();
            }
        }

        public void ActivateSlice(Vector3 force, Plane slicePlane, bool disable)
        {
            this.force = force;
            this.disable = disable;
            
            foreach (var attachment in ragdollAttachments)
            {
                attachment.Activate(force, disable);
            }
            
            Slice(slicePlane, gameObject.GetInstanceID(), null);
        }

        IEnumerator IEDisable()
        {
            yield return new WaitForSeconds(2f);
            Deactivate();
            while (transform.position.y > -10)
            {
                transform.position += Vector3.down * 10 * Time.deltaTime;
                yield return true;
            }
            gameObject.SetActive(false);
        }
        
        protected override AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
	    {
		    var skinnedRenderer = renderer as SkinnedMeshRenderer;
		    if (skinnedRenderer != null)
		    {
			    var result = new AdapterAndMesh();
			    result.mesh = skinnedRenderer.sharedMesh;
			    result.adapter = new BzSliceSkinnedMeshAddapter(skinnedRenderer);
			    return result;
		    }
		    var meshRenderer = renderer as MeshRenderer;
		    if (meshRenderer != null)
		    {
			    var result = new AdapterAndMesh();
			    result.mesh = meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
			    result.adapter = new BzSliceMeshFilterAddapter(result.mesh.vertices, meshRenderer.gameObject);
			    return result;
		    }

		    return null;
	    }
	    
	    protected override BzSliceTryData PrepareData(Plane plane)
		{
			var componentManager = new CharacterComponentManagerFast(gameObject, plane, colliders);

			return new BzSliceTryData()
			{
				componentManager = componentManager,
				plane = plane,
				addData = null,
			};
		}

		protected override void OnSliceFinished(BzSliceTryResult result)
		{
            Dacoder.Log("Sliced", result.sliced);
            
            this.Delay(2, () =>
			{
				if (result.sliced)
				{
					ConvertToRagdoll(result.outObjectNeg, Vector3.zero, disable);
                    ConvertToRagdoll(result.outObjectPos, Vector3.up * force.magnitude * 0.75f, disable);
                }
				else
				{
					ConvertToRagdoll(gameObject, Vector3.zero, disable);
				}
			});
		}

		void ConvertToRagdoll(GameObject result, Vector3 forceOffset, bool disable)
		{
			var animator = result.GetComponent<Animator>();
			if (animator)
			{
				animator.enabled = false;
            }
            
            var ragdoll = result.GetComponent<Ragdoll>();
            // var joints = result.GetComponentsInChildren<ConfigurableJoint>();
            ragdoll.rigidbodies = result.GetComponentsInChildren<Rigidbody>();
            ragdoll.colliders = result.GetComponentsInChildren<Collider>();
            ragdoll._ragdollAttachments = null;
            ragdoll.Activate(force + forceOffset, disable);
		}

        void Reset()
        {
	        Setup();
        }

        [ContextMenu("Setup")]
        void Setup()
        {
	        string[] hipNames = {"hip", "pelvis"};
            string[] spineNames = {"spine_02"};
            string[] chestNames = {"chest", "spine_03"};
            string[] headNames = {"head"};
            string[] upperArmLNames = {"upperarm_l"};
            string[] lowerArmLNames = {"lowerarm_l"};
            string[] handLNames = {"hand_l"};
            string[] upperArmRNames = {"upperarm_r"};
            string[] lowerArmRNames = {"lowerarm_r"};
            string[] handRNames = {"hand_r"};
            string[] upperLegLNames = {"upperleg_l", "thigh_l"};
            string[] lowerLegLNames = {"lowerleg_l", "calf_l"};
            string[] footLNames = {"foot_l"};
            string[] upperLegRNames = {"upperleg_r", "thigh_r"};
            string[] lowerLegRNames = {"lowerleg_r", "calf_r"};
            string[] footRNames = {"foot_r"};
            
            var transforms = GetComponentsInChildren<Transform>();
            hip = transforms.FirstOrDefault(t => Array.IndexOf(hipNames, t.name) != -1);
            spine = transforms.FirstOrDefault(t => Array.IndexOf(spineNames, t.name) != -1);
            chest = transforms.FirstOrDefault(t => Array.IndexOf(chestNames, t.name) != -1);
            head = transforms.FirstOrDefault(t => Array.IndexOf(headNames, t.name) != -1);
            upperArmL = transforms.FirstOrDefault(t => Array.IndexOf(upperArmLNames, t.name) != -1);
            lowerArmL = transforms.FirstOrDefault(t => Array.IndexOf(lowerArmLNames, t.name) != -1);
            handL = transforms.FirstOrDefault(t => Array.IndexOf(handLNames, t.name) != -1);
            upperArmR = transforms.FirstOrDefault(t => Array.IndexOf(upperArmRNames, t.name) != -1);
            lowerArmR = transforms.FirstOrDefault(t => Array.IndexOf(lowerArmRNames, t.name) != -1);
            handR = transforms.FirstOrDefault(t => Array.IndexOf(handRNames, t.name) != -1);
            upperLegL = transforms.FirstOrDefault(t => Array.IndexOf(upperLegLNames, t.name) != -1);
            lowerLegL = transforms.FirstOrDefault(t => Array.IndexOf(lowerLegLNames, t.name) != -1);
            footL = transforms.FirstOrDefault(t => Array.IndexOf(footLNames, t.name) != -1);
            upperLegR = transforms.FirstOrDefault(t => Array.IndexOf(upperLegRNames, t.name) != -1);
            lowerLegR = transforms.FirstOrDefault(t => Array.IndexOf(lowerLegRNames, t.name) != -1);
            footR = transforms.FirstOrDefault(t => Array.IndexOf(footRNames, t.name) != -1);

            AddComponents(hip, out var hipRigid, out var hipJoint, 0.2f, totalWeight * 0.137f, new Vector3(0, 0.1f, 0),
            ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
            ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
            0, 0, 0, 0);
            AddComponents(spine, out var spineRigid, out var spineJoint, 0.15f, totalWeight * 0.131f, new Vector3(0, 0.075f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited,
                -35, 60, 45, 25);
            AddComponents(chest, out var chestRigid, out var chestJoint, 0.2f, totalWeight * 0.201f, new Vector3(0, 0.1f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited,
                -35, 60, 45, 25);
            AddComponents(head, out var headRigid, out var headJoint, 0.3f, totalWeight * 0.0826f, new Vector3(0, 0.15f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited,
                -60, 40, 70, 30);
            AddComponents(upperArmL, out var upperArmLRigid, out var upperArmLJoint, 0.1f, totalWeight * 0.0325f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                0, 0, 0, 0);
            AddComponents(lowerArmL, out var lowerArmLRigid, out var lowerArmLJoint, 0.1f, totalWeight * 0.0187f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                0, 0, 0, 0);
            AddComponents(handL, out var handLRigid, out var handLJoint, 0.1f, totalWeight * 0.0065f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                0, 0, 0, 0);
            AddComponents(upperArmR, out var upperArmRRigid, out var upperArmRJoint, 0.1f, totalWeight * 0.0325f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                0, 0, 0, 0);
            AddComponents(lowerArmR, out var lowerArmRRigid, out var lowerArmRJoint, 0.1f, totalWeight * 0.0187f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                0, 0, 0, 0);
            AddComponents(handR, out var handRRigid, out var handRJoint, 0.1f, totalWeight * 0.0065f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                0, 0, 0, 0);
            AddComponents(upperLegL, out var upperLegLRigid, out var upperLegLJoint, 0.1f, totalWeight * 0.105f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited,
                0, 0, 26, 90);
            AddComponents(lowerLegL, out var lowerLegLRigid, out var lowerLegLJoint, 0.1f, totalWeight * 0.0475f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                -177, 0, 0, 0);
            AddComponents(footL, out var footLRigid, out var footLJoint, 0.1f, totalWeight * 0.0143f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                -27, 42, 0, 0);
            AddComponents(upperLegR, out var upperLegRRigid, out var upperLegRJoint, 0.1f, totalWeight * 0.105f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Free, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited,
                0, 0, 26, 90);
            AddComponents(lowerLegR, out var lowerLegRRigid, out var lowerLegRJoint, 0.1f, totalWeight * 0.0475f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                -177, 0, 0, 0);
            AddComponents(footR, out var footRRigid, out var footRJoint, 0.1f, totalWeight * 0.0143f, new Vector3(0, 0.05f, 0),
                ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked,
                ConfigurableJointMotion.Limited, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free,
                -27, 42, 0, 0);

            spineJoint.connectedBody = hipRigid;
            chestJoint.connectedBody = spineRigid;
            headJoint.connectedBody = chestRigid;
            upperArmLJoint.connectedBody = chestRigid;
            lowerArmLJoint.connectedBody = upperArmLRigid;
            handLJoint.connectedBody = lowerArmLRigid;
            upperArmRJoint.connectedBody = chestRigid;
            lowerArmRJoint.connectedBody = upperArmRRigid;
            handRJoint.connectedBody = lowerArmRRigid;
            upperLegLJoint.connectedBody = hipRigid;
            lowerLegLJoint.connectedBody = upperLegLRigid;
            footLJoint.connectedBody = lowerLegLRigid;
            upperLegRJoint.connectedBody = hipRigid;
            lowerLegRJoint.connectedBody = upperLegRRigid;
            footRJoint.connectedBody = lowerLegRRigid;

            var joints = GetComponentsInChildren<ConfigurableJoint>();
            rigidbodies = joints.Select(j => j.gameObject.GetComponent<Rigidbody>()).ToArray();
            colliders = joints.Select(j => j.gameObject.GetComponent<Collider>()).ToArray();
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        void AddComponents(Transform t, out Rigidbody rigid, out ConfigurableJoint joint, float r, float mass, Vector3 offset,
            ConfigurableJointMotion xMotion, ConfigurableJointMotion yMotion, ConfigurableJointMotion zMotion,
            ConfigurableJointMotion angularXMotion, ConfigurableJointMotion angularYMotion, ConfigurableJointMotion angularZMotion,
            float lowAngularXLimit, float highAngularXLimit, float angularYLimit, float angularZLimit)
        {
            rigid = t.gameObject.GetComponent<Rigidbody>();
            if (!rigid)
            {
                rigid = t.gameObject.AddComponent<Rigidbody>();
            }

            var collider = t.gameObject.GetComponent<SphereCollider>();
            if (!collider)
            {
                collider = t.gameObject.AddComponent<SphereCollider>();
            }

            joint = t.gameObject.GetComponent<ConfigurableJoint>();
            if (!joint)
            {
                joint = t.gameObject.AddComponent<ConfigurableJoint>();
            }
            
            rigid.mass = mass;
            collider.radius = r / t.lossyScale.x;
            collider.center = offset / t.lossyScale.x;
            
            var d = joint.angularXDrive;
            d.positionSpring = 1;
            joint.angularXDrive = d;
            
            d = joint.angularYZDrive;
            d.positionSpring = 1;
            joint.angularYZDrive = d;
            
            joint.xMotion = xMotion;
            joint.yMotion = yMotion;
            joint.zMotion = zMotion;

            joint.angularXMotion = angularXMotion;
            joint.angularYMotion = angularYMotion;
            joint.angularZMotion = angularZMotion;

            joint.lowAngularXLimit = new SoftJointLimit() {limit = lowAngularXLimit};
            joint.highAngularXLimit = new SoftJointLimit() {limit = highAngularXLimit};
            joint.angularYLimit = new SoftJointLimit() {limit = angularYLimit};
            joint.angularZLimit = new SoftJointLimit() {limit = angularZLimit};
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(rigid);
            EditorUtility.SetDirty(collider);
            EditorUtility.SetDirty(joint);
#endif
        }
    }
}
#endif