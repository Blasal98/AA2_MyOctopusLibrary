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
        float[] _gradientTail;


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
            _gradientTail = new float[_tail.Bones.Length];

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                _tail.Bones[0].rotation = Quaternion.identity;
            }
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                _solutionsTail[i] = 0;
                if(i%2 == 0)_axisTail[i] = new Vector3(1,0,0);
                else _axisTail[i] = new Vector3(0,0,1);
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
            if ((_tail.EndEffector[0].position - tailTarget.position).magnitude < 5f)
            {
                //TODO

                if (ErrorFunction(_solutionsTail, _axisTail, _offsetTail, tailTarget.position) > 0.1f)
                {
                    for (int i = 0; i < _tail.Bones.Length; i++)
                    {
                        _gradientTail[i] = gradientFunction(tailTarget.position, _solutionsTail, _axisTail, _offsetTail, i, 0.1f);
                    }
                    for (int i = 0; i < _tail.Bones.Length; i++)
                    {
                        _solutionsTail[i] = _solutionsTail[i] - 1 * _gradientTail[i];
                    }
                    FW_Tail();
                }
            }
            Debug.Log(ErrorFunction(_solutionsTail, _axisTail, _offsetTail, tailTarget.position));
            Debug.Log((_tail.EndEffector[0].position - tailTarget.position).magnitude);
            Debug.Log(" ");
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {

        }
        private float gradientFunction(Vector3 target, float[] solutions, Vector3[] axis, Vector3[] offsets, int i, float delta)
        {
            float gradient = 0;
            float auxAngle = solutions[i];
            float f_x = ErrorFunction(solutions, axis, offsets, target);
            solutions[i] += delta;
            float f_x_plus = ErrorFunction(solutions, axis, offsets, target);
            gradient = (f_x_plus - f_x) / delta;
            solutions[i] = auxAngle;

            return gradient;
        }
        private void FW_Tail()
        {
            Quaternion rotation = _tail.Bones[0].transform.rotation;

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                 rotation *= Quaternion.AngleAxis(_solutionsTail[i], _axisTail[i]);
                _tail.Bones[i].rotation = rotation;
            }
        }
        private Vector3 ForwardKinematics(float[] solutions, Vector3[] axis, Vector3[] offsets) 
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;
            Quaternion rotation = _tail.Bones[0].transform.rotation;

            for(int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(solutions[i - 1], axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * offsets[i];
                prevPoint = nextPoint;
            }
            return prevPoint;
        }
        private float ErrorFunction(float[] solutions, Vector3[] axis, Vector3[] offsets, Vector3 target)
        {
            return (ForwardKinematics(solutions,axis,offsets) - target).magnitude;
        }
        #endregion
    }
}
