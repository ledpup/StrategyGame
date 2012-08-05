using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class Battle
    {
        private List<Player> _players;
        List<Unit> Units;
        Dictionary<Player, List<Unit>> PlayerUnits;
        public List<ICommand> Events;
        IEnumerable<Player> Winners;

        public Battle(List<Unit> units)
        {
            _players = units.Select(u => u.Player).Distinct().ToList();
            Units = units;

            PlayerUnits = new Dictionary<Player, List<Unit>>();
            _players.ForEach(x => PlayerUnits.Add(x, Units.Where(u => u.Player == x).ToList()));

            Events = new List<ICommand>();
        }

        public void AssignCasulties()
        {
            var loses = new Dictionary<Player,float>();

            _players.ForEach(p =>
            {
                var damage = PlayerUnits[p].Sum(ou => ou.CombatStrength);
                loses.Add(p, damage);

                _players.Where(o => o != p)
                    .ToList()
                    .ForEach(o => PlayerUnits[o]
                                    .ToList()
                                    .ForEach(u => Events.Add(new ChangeQuantityCommand(u, -damage))));
                        

            });

            Winners = loses.Where(x => x.Value == loses.Min(l => l.Value)).Select(x => x.Key);
        }

        internal void ReduceMorale()
        {
            _players.ForEach(x => 
            {
                var moraleDecrease = .2f;
                if (Winners.Contains(x))
                    moraleDecrease = .1f;

                PlayerUnits[x].ForEach(u => Events.Add(new ChangeMoraleCommand(u, -moraleDecrease)));
            });
        }

        internal void ReduceStamina()
        {
            Units.ForEach(x => Events.Add(new ChangeMoraleCommand(x, -.2f)));
        }
    }
}
