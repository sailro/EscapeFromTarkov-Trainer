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

		public static readonly List<string[]> Connections = new()
		{
			new [] {Pelvis, LThigh1}, new [] {LThigh1, LThigh2}, new [] {LThigh2, LCalf}, new [] {LCalf, LFoot}, new [] {LFoot, LToe}, new [] {Pelvis, RThigh1}, new [] {RThigh1, RThigh2}, new [] {RThigh2, RCalf},
			new [] {RCalf, RFoot}, new [] {RFoot, RToe}, new [] {Pelvis, Spine1}, new [] {Spine1, Spine2}, new [] {Spine2, Spine3}, new [] {Spine3, Neck}, new [] {Neck, Head}, new [] {Spine3, LCollarbone},
			new [] {LCollarbone, LForearm1}, new [] {LForearm1, LForearm2}, new [] {LForearm2, LForearm3}, new [] {LForearm3, LPalm}, new [] {LPalm, LDigit11}, new [] {LDigit11, LDigit12},
			new [] {LDigit12, LDigit13}, new [] {LPalm, LDigit21}, new [] {LDigit21, LDigit22}, new [] {LDigit22, LDigit23}, new [] {LPalm, LDigit31}, new [] {LDigit31, LDigit32}, new [] {LDigit32, LDigit33},
			new [] {LPalm, LDigit41}, new [] {LDigit41, LDigit42}, new [] {LDigit42, LDigit43}, new [] {LPalm, LDigit51}, new [] {LDigit51, LDigit52}, new [] {LDigit52, LDigit53}, new [] {Spine3, RCollarbone},
			new [] {RCollarbone, RForearm1}, new [] {RForearm1, RForearm2}, new [] {RForearm2, RForearm3}, new [] {RForearm3, RPalm}, new [] {RPalm, RDigit11}, new [] {RDigit11, RDigit12},
			new [] {RDigit12, RDigit13}, new [] {RPalm, RDigit11}, new [] {RDigit21, RDigit22}, new [] {RDigit22, RDigit23}, new [] {RPalm, RDigit11}, new [] {RDigit31, RDigit32}, new [] {RDigit32, RDigit33},
			new [] {RPalm, RDigit11}, new [] {RDigit41, RDigit42}, new [] {RDigit42, RDigit43}, new [] {RPalm, RDigit11}, new [] {RDigit51, RDigit52}, new [] {RDigit52, RDigit53}
		};

		public static void RenderBone(Dictionary<string, Transform> bones, string from, string to, float thickness, Color color, Camera camera)
		{
			RenderBone(bones[from].position, bones[to].position, thickness, color, camera);
		}

		public static void RenderBone(Vector3 fromPosition, Vector3 toPosition, float thickness, Color color, Camera camera)
		{
			var fromScreenPosition = camera.WorldPointToScreenPoint(fromPosition);
			var toScreenPosition = camera.WorldPointToScreenPoint(toPosition);

			Render.DrawLine(new Vector2(fromScreenPosition.x, fromScreenPosition.y), new Vector2(toScreenPosition.x, toScreenPosition.y), thickness, color);
		}

		public static void RenderBones(Player player, float thickness, Color color, Camera camera)
		{
			var skeleton = player.PlayerBody.SkeletonRootJoint;
			if (skeleton == null)
				return;

			var bones = skeleton.Bones;
			if (bones == null)
				return;

			foreach (var connection in Connections)
				RenderBone(bones, connection[0], connection[1], thickness, color, camera);

			var head = camera.WorldPointToScreenPoint(bones[Head].position);
			var neck = camera.WorldPointToScreenPoint(bones[Neck].position);
			var radius = Vector3.Distance(head, neck);

			Render.DrawCircle(new Vector2(head.x, head.y), radius, color, thickness, 8);
		}
	}
}
