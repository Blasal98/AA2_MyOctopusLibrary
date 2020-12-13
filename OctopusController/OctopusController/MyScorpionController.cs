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
        float _sizeTail;
        float _learningStep;
        float _deltaGradient;


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
            _sizeTail = 0;
            _learningStep = 10;
            _deltaGradient = 0.05f;

            Quaternion[] auxRotations = new Quaternion[_tail.Bones.Length];
            for (int i = 0; i < _tail.Bones.Length; i++) //Guardem Rotations i Posem els bones amb rotation 0
            {
                //auxRotations[i] = _tail.Bones[i].localRotation;
                _tail.Bones[i].rotation = Quaternion.identity;
            }
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                _solutionsTail[i] = 0;
                if (i == 1) _axisTail[i] = new Vector3(0, 0, 1); //que un pugui girar en x
                else _axisTail[i] = new Vector3(1, 0, 0);
                if (i != _tail.Bones.Length - 1) _offsetTail[i] = _tail.Bones[i + 1].position - _tail.Bones[i].position;
                else _offsetTail[i] = _tail.EndEffector[0].position - _tail.Bones[i].position;

                //Debug.Log(_solutionsTail[i] + " " + _axisTail[i] + " " + _offsetTail[i] + " " + _offsetTail[i].magnitude);
            }
            for (int i = 0; i < _tail.Bones.Length; i++) //Medim la cua i restaurem la posicio de la cua
            {
                _sizeTail += _offsetTail[i].magnitude;
                //_tail.Bones[i].localRotation = auxRotations[i];
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
            if ((_tail.Bones[0].position - tailTarget.position).magnitude <= _sizeTail) //si la bola i bone[0] estan a una distancia inferior a la mida de la cua
            {
                //TODO
                for(int j=0; j < 3; j++) {
                    if ((_tail.EndEffector[0].position - tailTarget.position).magnitude > 0.05f) //si el endeffector esta a mes de 0.1 unitatas de unity de la bola
                    {
                        for (int i = 0; i < _tail.Bones.Length; i++)
                        {
                            _gradientTail[i] = gradientFunction(tailTarget.position, _solutionsTail, _axisTail, _offsetTail, i, _deltaGradient); //gradients parcials
                        }
                        for (int i = 0; i < _tail.Bones.Length; i++)
                        {
                            _solutionsTail[i] = _solutionsTail[i] - _learningStep * _gradientTail[i]; //aplicar gradients a la solució
                        }
                        
                    }
                    //else Debug.Log("GOOOL");
                }
                FW_Tail(); //actualitzem la cua visualment
            }
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {

        }
        private float gradientFunction(Vector3 target, float[] solutions, Vector3[] axis, Vector3[] offsets, int i, float delta) //gradients parcials
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
        private void FW_Tail() //ForwardKinematics -> Mou la cua
        {
            Quaternion rotation = _tail.Bones[0].transform.rotation;

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                 rotation *= Quaternion.AngleAxis(_solutionsTail[i], _axisTail[i]);
                _tail.Bones[i].rotation = rotation;
            }
        }
        private Vector3 ForwardKinematics(float[] solutions, Vector3[] axis, Vector3[] offsets) //ForwardKinematics -> Calcula la posició del endeffector
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;
            Quaternion rotation = _tail.Bones[0].GetComponentInParent<Transform>().rotation; 

            for(int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(solutions[i - 1], axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * offsets[i];
                prevPoint = nextPoint;
            }
            return prevPoint;
        }
        private float ErrorFunction(float[] solutions, Vector3[] axis, Vector3[] offsets, Vector3 target) //La nostra errorFunction calcula la distancia entre endeffector i target
        {
            return (ForwardKinematics(solutions,axis,offsets) - target).magnitude;
        }
        #endregion
    }
}
