using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public class Spy
    {
        #region static
        public static Spy Instance;
        public static readonly CustomRoles role = CustomRoles.Spy;
        static readonly int optionId = 21000;
        #region CustomOptions
        private static CustomOption opt_SpyCanVent;
        private static CustomOption opt_ImpostorCanKillSpy;
        private static CustomOption opt_SheriffCanKillSpy;
        #endregion
        #region OptionGetter
        public static bool IsEnable => role.IsEnable();
        public static float RoleChance => role.GetChance();
        public static int RoleCount => role.GetCount();
        public static bool SpyCanVent => opt_SpyCanVent.GetBool();
        public static bool ImpostorCanKillSpy => opt_ImpostorCanKillSpy.GetBool();
        public static bool SheriffCanKillSpy => opt_SheriffCanKillSpy.GetBool();
        #endregion
        public static void SetupCustomOption()
        {
            //21000~21001
            Options.SetupSingleRoleOptions(optionId, role, 1);
            CustomOption parent = Options.CustomRoleSpawnChances[role];
            int i = 10; //以降、21010から1ずつ上がっていく。
            opt_SpyCanVent = CustomOption.Create(optionId + i++, Color.white, "SpyCanVent", true, parent);
            opt_ImpostorCanKillSpy = CustomOption.Create(optionId + i++, Color.white, "ImpostorCanKillSpy", true, parent);
            opt_SheriffCanKillSpy = CustomOption.Create(optionId + i++, Color.white, "SheriffCanKillSpy", false, parent);
        }
        public static void AssignRoleForRandomPlayer(ref List<PlayerControl> AllPlayers, CustomRpcSender sender)
        {
            System.Random rand = new();
            if (role.IsEnable())
            {
                for (int i = 0; i < role.GetCount(); i++)
                {
                    if (AllPlayers.Count <= 0) break;
                    var target = AllPlayers[rand.Next(0, AllPlayers.Count)];
                    AllPlayers.Remove(target);
                    Spy.Assign(target);
                    //Desync開始 そういえば最近ホストもDesyncするようになったんだってね。
                    if (target.PlayerId != 0)
                    {
                        int clientId = target.GetClientId();
                        //target視点: target = クルー or エンジニア [TODO]
                        sender.RpcSetRole(target, RoleTypes.Engineer, clientId);
                        //target視点: 他プレイヤー = 科学者
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == target) continue;
                            sender.RpcSetRole(pc, RoleTypes.Scientist, clientId);
                        }
                        //他プレイヤー: target = インポスター
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == target) continue;
                            if (pc.PlayerId == 0) target.SetRole(RoleTypes.Impostor); //ホスト視点用
                            else sender.RpcSetRole(target, RoleTypes.Impostor, pc.GetClientId());
                        }
                    }
                    else
                    {
                        //ホストは代わりに自視点エンジニア, 他視点インポスターにする
                        target.SetRole(RoleTypes.Engineer); //ホスト視点用
                        sender.RpcSetRole(target, RoleTypes.Impostor);
                    }
                    target.Data.IsDead = true;
                }
            }
        }
        #endregion

        public PlayerControl player;
        private Spy(PlayerControl player)
        {
            this.player = player;
        }
        public static Spy Assign(PlayerControl player)
        {
            var role = new Spy(player);
            Main.AllPlayerCustomRoles[player.PlayerId] = Spy.role;
            return role;
        }
        public void Init()
        {
        }
        public void FixedUpdate()
        {

        }
        public void OnStartMeeting()
        {

        }
    }
}