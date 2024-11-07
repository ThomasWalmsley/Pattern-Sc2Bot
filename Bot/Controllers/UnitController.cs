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

        public List<Action> Attack(List<Unit> units, Vector3 target) 
        {
            List<Action> actions =new List<Action>();

            foreach (var unit in units) 
            {
                //check if the unit can fire
                //Current frame
                //last attack frame
                if (unit.RawUnitData == null)
                {
                    // Handle null RawUnitData
                    continue;
                }
                var weaponCooldown = unit.RawUnitData.WeaponCooldown;
                //var cooldown = Controller.FRAMES_PER_SECOND * (weapon.Speed / 1.4f)
                GraphicalDebug.DrawText(weaponCooldown.ToString(), unit,10);
            }


            

            return actions;
        }
    }
}
