using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public interface ITeamManager
    {
        [Serializable]
        public struct TeamInfo
        {
            public string defaultName;
            public Color color;
            public int id;
        }

        public IReadOnlyList<TeamInfo> Teams { get; }

        public int MainTeam1Id { get; }
        public int MainTeam2Id { get; }
    }

    public static class TeamManagerExtensions
    {
        public static ITeamManager.TeamInfo? GetTeamById(this ITeamManager teamManager, int id)
        {
            // fast, no alloc

            var teams = teamManager.Teams;
            int teamsCount = teams.Count;

            for (int i = 0; i < teamsCount; i++)
            {
                var team = teams[i];
                if (team.id == id)
                    return team;
            }

            return null;
        }
    }
}
