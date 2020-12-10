using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        float[] _solutionsTail;
        Vector3[] _axisTail;
        Vector3[] _offsetTail;


        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        
        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
            }

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
            _solutionsTail = new float[_tail.Bones.Length];
            _axisTail = new Vector3[_tail.Bones.Length];
            _offsetTail = new Vector3[_tail.Bones.Length];

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                _tail.Bones[0].rotation = Quaternion.identity;
            }
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                _solutionsTail[i] = 0;
                _axisTail[i] = new Vector3(1,0,0);
                if (i < _tail.Bones.Length - 1) _offsetTail[i] = _tail.Bones[i + 1].position - _tail.Bones[i].position;
                else _offsetTail[i] = _tail.EndEffector[0].position - _tail.Bones[i].position;
            }

        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()//la que update unity
        {
            updateTail();
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            //
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            FKTail(_solutionsTail,_axisTail);
            if ((_tail.EndEffector[0].position - tailTarget.position).magnitude < 1f)
            {
               //TODO


            }
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {

        }
        private void gradientFunction(Vector3 target, float[] solutions, int i, float delta)
        {
            float gradient = 0;
        }
        //aixo es basicament forward kinematics, pero podriem fer punter de funcio
        private void ErrorFunction(float[] solutions, Vector3[] axis, Vector3[] offsets) 
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;
            Quaternion rotation = _tail.Bones[0].transform.rotation;

            for(int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(solutions[i - 1], axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * offsets[i];
                prevPoint = nextPoint;
            }
            
        }
        private void FKTail(float[] solutions, Vector3[] axis)
        {
            
        }
        #endregion
    }
}
