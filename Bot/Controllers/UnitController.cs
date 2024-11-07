using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Action = SC2APIProtocol.Action;

namespace Bot.Controllers
{
    internal class UnitController : BaseController
    {
        public UnitController() { }

        public Action Attack(Unit unit, Vector3 target) 
        {
            var action = Controller.CreateRawUnitCommand(Abilities.ATTACK);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);
            Controller.AddAction(action);


            return action;
        }
    }
}
