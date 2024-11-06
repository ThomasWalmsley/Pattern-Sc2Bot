using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using SC2APIProtocol;

namespace Bot

    //ISSUES
    //Workers are stored as Units in here. But the unit should only be stored in the unit tracker. we should only store the tags
    //here. 

{
    class TownHallSupervisor
    {
        public Unit townhall;
        public List<Unit> workers = new List<Unit>();
        public List<ulong> workertags = new List<ulong>();
        public List<Unit> far_mineralfields = new List<Unit>();
        public List<Unit> close_mineralfields = new List<Unit>();
        public List<Unit> gas = new List<Unit>();
        public List<Unit> gasbuildings = new List<Unit>();
        public int age = 0;
        public int watched_worker = 0;

        public bool printed = false;//get rid of soon

        public TownHallSupervisor(Unit TownHall) { townhall = TownHall; }

        public void onFrame() 
        {
            draw_all();
            request_mineralfields();
            request_workers();
            Updateunits();
            //draw_all();
            //if (Controller.frame %100 == 0) { assign_workers_to_mineralfields(); }
            assign_workers_to_mineralfields();
            //unassign_workers();
            if (Controller.frame % 10 == 0) { distibute_workers(); }
        }

        public void Updateunits() 
        {
          //  workers.Clear();
          //  foreach(var tag in workertags) 
          //  {
          //      workers.Add(UnitsTracker.UnitsByTag[tag]);
          //  }

            //Don't use this :(
            //Crashes when a unit dies because it isn't removed from these lists
            //foreach(var wkr in workers) { wkr.Update(); }
            //foreach(var mf in far_mineralfields) { mf.Update(); }
            //foreach(var mf in close_mineralfields) { mf.Update(); }
            //string posit = workers[watched_worker].Position.ToString();
            //Console.WriteLine(posit);
        }


        public void draw_all() 
        {
            //draw townhall
            //Controller.gdebug.DrawCube(townhall, 5);
            //draw workers

            foreach (var worker in workers)
            {
                //Controller.gdebug.DrawCube(worker, 1);
                if (worker.AssignedMineralPatch != null)
                {
                    GraphicalDebug.DrawLine(worker,worker.AssignedMineralPatch);
                }
            }
            //draw mineral fields
            foreach (var mf in close_mineralfields)
            {
                //Controller.gdebug.DrawSphere(mf, 1);
                GraphicalDebug.DrawText(mf.Workers_assigned.Count().ToString(), mf, 20);
                //Controller.gdebug.DrawText(mf.tag.ToString(), mf, 20);
            }
            foreach (var mf in far_mineralfields) 
            {
                GraphicalDebug.DrawText(mf.Workers_assigned.Count().ToString(), mf, 20);
            }

        }

        public void request_workers() 
        {
            //currently takes control of all nearby workers
            //should request workers which are then assigned to this supervisor by another manager
            List<Unit> nearWorkers = Controller.GetUnitsInRange(townhall.Position, Units.Workers, 10, onlyVisible: true, alliance: Alliance.Self);
            foreach(var wkr in nearWorkers) 
            {
                if (!(workertags.Contains(wkr.Tag)))
                {
                    workers.Add(wkr);
                    workertags.Add(wkr.Tag);
                    Console.WriteLine("Add worker");
                }
            }
        }
        public void request_mineralfields() 
        {
            if (Controller.frame != 0) { return; }
            Console.WriteLine("request mineral fields");
            far_mineralfields = Controller.GetUnitsInRange(townhall.Position, Units.MineralFields_750, 10, onlyVisible: true, alliance: Alliance.Neutral);
            close_mineralfields = Controller.GetUnitsInRange(townhall.Position, Units.MineralFields_Close, 10, onlyVisible: true, alliance: Alliance.Neutral);
        }

        public void assign_workers_to_mineralfields() 
        {
            //goal is to get 2 workers per mineral patch, with close mineral patches prioritised

            //assign closest workers to each mineral field
            foreach (var mf in close_mineralfields) 
            {
                //List<Unit> closestWorkers = availableWorkers.Where(unit => unit.assignedMineralPatch == null).OrderBy(unit => Vector3.Distance(townhall.position, unit.position)).ToList();
                List<Unit> closestWorkers = workers.
                    Where(unit => unit.AssignedMineralPatch == null).
                    OrderBy(unit => Vector3.Distance(townhall.Position, unit.Position)).
                    ToList();
                while (mf.Workers_assigned.Count < 2 && closestWorkers.Count() >0) 
                {
                    //assign closest worker to mineral patch
                    closestWorkers[0].AssignedMineralPatch = mf;
                    mf.Workers_assigned.Add(closestWorkers[0]);
                    closestWorkers.RemoveAt(0);
                }
            }
            foreach (var mf in far_mineralfields) 
            {
                List<Unit> closestWorkers = workers.Where(unit => unit.AssignedMineralPatch == null).OrderBy(unit => Vector3.Distance(townhall.Position, unit.Position)).ToList();
                while (mf.Workers_assigned.Count < 2 && closestWorkers.Count() > 0)
                {
                    //assign closest worker to mineral patch
                    closestWorkers[0].AssignedMineralPatch = mf;
                    mf.Workers_assigned.Add(closestWorkers[0]);
                    closestWorkers.RemoveAt(0);
                }
            }

            

        }

        public void unassign_workers() 
        {
            foreach(var wkr in workers) 
            {
                if (wkr.Order.AbilityId == Abilities.RETURN_MINERALS) { continue; }
                if (wkr.Order.AbilityId == Abilities.GATHER_MINERALS) { continue; }
                if (wkr.AssignedMineralPatch == null) { continue; }
                wkr.AssignedMineralPatch.AssignedWorkers = wkr.AssignedMineralPatch.AssignedWorkers - 1;
                wkr.AssignedMineralPatch = null;
            }
        }

        public void distibute_workers() 
        {
            foreach(var wkr in workers.Where(i => i.AssignedMineralPatch!=null)) 
            {
                if(wkr.Order.AbilityId == Abilities.RETURN_MINERALS) {continue; }
                if (wkr.AssignedMineralPatch.Tag == wkr.Order.TargetUnitTag){continue; }
                if (wkr.Order.AbilityId >= 318 && wkr.Order.AbilityId <= 347) {continue; }//Terran Build Abilities
                //Console.WriteLine(wkr.AssignedMineralPatch.Tag);
                //if (!(wkr.AssignedMineralPatch.Tag == wkr.Order.TargetUnitTag)) { Console.WriteLine("redirecting worker"); }
                wkr.Smart(wkr.AssignedMineralPatch);
                //wkr.Smart(wkr.AssignedMineralPatch);
            }
        }

    }
}
