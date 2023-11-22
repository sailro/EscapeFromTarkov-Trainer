using System.Collections.Generic;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	internal static class Bones
	{
		public const string Pelvis = "Root_Joint/Base HumanPelvis";
		public const string Spine1 = Pelvis + "/Base HumanSpine1";
		public const string Spine2 = Spine1 + "/Base HumanSpine2";
		public const string Spine3 = Spine2 + "/Base HumanSpine3";
		public const string Neck = Spine3 + "/Base HumanNeck";
		public const string Head = Neck + "/Base HumanHead";
		
		public const string LThigh1 = Pelvis + "/Base HumanLThigh1";
		public const string LThigh2 = LThigh1 + "/Base HumanLThigh2";
		public const string LCalf = LThigh2 + "/Base HumanLCalf";
		public const string LFoot = LCalf + "/Base HumanLFoot";
		public const string LToe = LFoot + "/Base HumanLToe";
		public const string LCollarbone = Spine3 + "/Base HumanRibcage/Base HumanLCollarbone";
		public const string LForearm1 = LCollarbone + "/Base HumanLUpperarm/Base HumanLForearm1";
		public const string LForearm2 = LForearm1 + "/Base HumanLForearm2";
		public const string LForearm3 = LForearm2 + "/Base HumanLForearm3";
		public const string LPalm = LForearm3 + "/Base HumanLPalm";
		public const string LDigit11 = LPalm + "/Base HumanLDigit11";
		public const string LDigit12 = LDigit11 + "/Base HumanLDigit12";
		public const string LDigit13 = LDigit12 + "/Base HumanLDigit13";
		public const string LDigit21 = LPalm + "/Base HumanLDigit21";
		public const string LDigit22 = LDigit21 + "/Base HumanLDigit22";
		public const string LDigit23 = LDigit22 + "/Base HumanLDigit23";
		public const string LDigit31 = LPalm + "/Base HumanLDigit31";
		public const string LDigit32 = LDigit31 + "/Base HumanLDigit32";
		public const string LDigit33 = LDigit32 + "/Base HumanLDigit33";
		public const string LDigit41 = LPalm + "/Base HumanLDigit41";
		public const string LDigit42 = LDigit41 + "/Base HumanLDigit42";
		public const string LDigit43 = LDigit42 + "/Base HumanLDigit43";
		public const string LDigit51 = LPalm + "/Base HumanLDigit51";
		public const string LDigit52 = LDigit51 + "/Base HumanLDigit52";
		public const string LDigit53 = LDigit52 + "/Base HumanLDigit53";

		public const string RThigh1 = Pelvis + "/Base HumanRThigh1";
		public const string RThigh2 = RThigh1 + "/Base HumanRThigh2";
		public const string RCalf = RThigh2 + "/Base HumanRCalf";
		public const string RFoot = RCalf + "/Base HumanRFoot";
		public const string RToe = RFoot + "/Base HumanRToe";
		public const string RCollarbone = Spine3 + "/Base HumanRibcage/Base HumanRCollarbone";
		public const string RForearm1 = RCollarbone + "/Base HumanRUpperarm/Base HumanRForearm1";
		public const string RForearm2 = RForearm1 + "/Base HumanRForearm2";
		public const string RForearm3 = RForearm2 + "/Base HumanRForearm3";
		public const string RPalm = RForearm3 +"/Base HumanRPalm";
		public const string RDigit11 = RPalm + "/Base HumanRDigit11";
		public const string RDigit12 = RDigit11 + "/Base HumanRDigit12";
		public const string RDigit13 = RDigit12 + "/Base HumanRDigit13";
		public const string RDigit21 = RPalm + "/Base HumanRDigit21";
		public const string RDigit22 = RDigit21 +"/Base HumanRDigit22";
		public const string RDigit23 = RDigit22 + "/Base HumanRDigit23";
		public const string RDigit31 = RPalm + "/Base HumanRDigit31";
		public const string RDigit32 = RDigit31 + "/Base HumanRDigit32";
		public const string RDigit33 = RDigit32 + "/Base HumanRDigit33";
		public const string RDigit41 = RPalm + "/Base HumanRDigit41";
		public const string RDigit42 = RDigit41 + "/Base HumanRDigit42";
		public const string RDigit43 = RDigit42 + "/Base HumanRDigit43";
		public const string RDigit51 = RPalm + "/Base HumanRDigit51";
		public const string RDigit52 = RDigit51 + "/Base HumanRDigit52";
		public const string RDigit53 = RDigit52 + "/Base HumanRDigit53";

		public static readonly List<string[]> Connections =
		[
			[Pelvis, LThigh1], [LThigh1, LThigh2], [LThigh2, LCalf], [LCalf, LFoot], [LFoot, LToe], [Pelvis, RThigh1], [RThigh1, RThigh2], [RThigh2, RCalf],
			[RCalf, RFoot], [RFoot, RToe], [Pelvis, Spine1], [Spine1, Spine2], [Spine2, Spine3], [Spine3, Neck], [Neck, Head], [Spine3, LCollarbone],
			[LCollarbone, LForearm1], [LForearm1, LForearm2], [LForearm2, LForearm3], [LForearm3, LPalm], [Spine3, RCollarbone],
			[RCollarbone, RForearm1], [RForearm1, RForearm2], [RForearm2, RForearm3], [RForearm3, RPalm] 
		];

		public static readonly List<string[]> FingerConnections =
		[
			[LPalm, LDigit11], [LDigit11, LDigit12], [LDigit12, LDigit13], [LPalm, LDigit21], [LDigit21, LDigit22], [LDigit22, LDigit23], [LPalm, LDigit31],
			[LDigit31, LDigit32], [LDigit32, LDigit33], [LPalm, LDigit41], [LDigit41, LDigit42], [LDigit42, LDigit43], [LPalm, LDigit51], [LDigit51, LDigit52],
			[LDigit52, LDigit53], [RPalm, RDigit11], [RDigit11, RDigit12],	[RDigit12, RDigit13], [RPalm, RDigit11], [RDigit21, RDigit22], [RDigit22, RDigit23],
			[RPalm, RDigit11], [RDigit31, RDigit32], [RDigit32, RDigit33],	[RPalm, RDigit11], [RDigit41, RDigit42], [RDigit42, RDigit43], [RPalm, RDigit11],
			[RDigit51, RDigit52], [RDigit52, RDigit53]
		];

		private static Vector2 GetScreenPosition(Camera camera, Vector3 position, bool isAiming)
		{
			return isAiming ? Players.ScopePointToScreenPoint(camera, position, true) : camera.WorldPointToScreenPoint(position);
		}

		public static void RenderBone(Dictionary<string, Transform> bones, string from, string to, float thickness, Color color, Camera camera, bool isAiming)
		{
			RenderBone(bones[from].position, bones[to].position, thickness, color, camera, isAiming);
		}

		public static void RenderBone(Vector3 fromPosition, Vector3 toPosition, float thickness, Color color, Camera camera, bool isAiming)
		{
			var fromScreenPosition = GetScreenPosition(camera, fromPosition, isAiming);
			var toScreenPosition = GetScreenPosition(camera, toPosition, isAiming);

			Render.DrawLine(fromScreenPosition, toScreenPosition, thickness, color);
		}

		public static void RenderBones(Player player, float thickness, Color color, Camera camera, bool isAiming, float distance)
		{
			var skeleton = player.PlayerBody.SkeletonRootJoint;
			if (skeleton == null)
				return;

			var bones = skeleton.Bones;
			if (bones == null)
				return;

			foreach (var connection in Connections)
				RenderBone(bones, connection[0], connection[1], thickness, color, camera, isAiming);

			if (distance < 75f)
				foreach (var finger in FingerConnections)
					RenderBone(bones, finger[0], finger[1], thickness, color, camera, isAiming);


			var head = GetScreenPosition(camera, bones[Head].position, isAiming);
			var neck = GetScreenPosition(camera, bones[Neck].position, isAiming);

			var radius = Vector2.Distance(head, neck);

			Render.DrawCircle(head, radius, color, thickness, 8);
		}

		public static void RenderBones(Player player, string[] connections, float thickness, Color color, Camera camera, bool isAiming)
		{
			var skeleton = player.PlayerBody.SkeletonRootJoint;
			if (skeleton == null)
				return;

			var bones = skeleton.Bones;
			if (bones == null)
				return;

			var numberOfConnections = connections.Length;
			for (int i = 0; i < numberOfConnections; i += 2)
			{
				RenderBone(bones, connections[i], connections[i + 1], thickness, color, camera, isAiming);
			}
		}

		public static void RenderHead(Player player, float thickness, Color color, Camera camera, bool isAiming)
		{
			var skeleton = player.PlayerBody.SkeletonRootJoint;
			if (skeleton == null)
				return;

			var bones = skeleton.Bones;
			if (bones == null)
				return;

			var head = GetScreenPosition(camera, bones[Head].position, isAiming);
			var neck = GetScreenPosition(camera, bones[Neck].position, isAiming);

			var radius = Vector2.Distance(head, neck);

			Render.DrawCircle(head, radius, color, thickness, 8);
		}

		public static void RenderFingers(Player player, float thickness, Color color, Camera camera, bool isAiming)
		{
			var skeleton = player.PlayerBody.SkeletonRootJoint;
			if (skeleton == null)
				return;

			var bones = skeleton.Bones;
			if (bones == null)
				return;

			foreach (var connection in FingerConnections)
				RenderBone(bones, connection[0], connection[1], thickness, color, camera, isAiming);
		}
	}
}
