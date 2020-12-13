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



        //LEGS
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];
        private Vector3[] copy;
        private float[] jointsLength;
        bool isMoving = false;
        int maxLegs = 6;
        float timeToMove = 0;

        #region public
        public void InitLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                legFutureBases[i] = LegFutureBases[i];
                legTargets[i] = LegTargets[i];
            }
            copy = new Vector3[_legs[0].Bones.Length];
            //
            jointsLength = new float[_legs[0].Bones.Length - 1];
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {

        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {
            isMoving = true;
            timeToMove = 0;
            animationRange = 5;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            updateLegPos();
            if (isMoving)
            {


                timeToMove += Time.deltaTime;

                if (timeToMove < animationRange)
                    updateLegPos();

                else
                    isMoving = false;


            }

        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            for (int i = 0; i < maxLegs; i++)
            {


                if ((Vector3.Distance(_legs[i].Bones[0].position, legFutureBases[i].position)) > 1)
                {
                    _legs[i].Bones[0].position = Vector3.Lerp(_legs[i].Bones[0].position, legFutureBases[i].position, 1.4f);
                }
                //update la pierna que necesitamos y no todas como antes
                updateLegs(i);
            }

        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {

        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs(int leg)
        {

            for (int i = 0; i <= _legs[0].Bones.Length - 1; i++)
            {
                copy[i] = _legs[leg].Bones[i].position;
            }

            for (int i = 0; i <= _legs[leg].Bones.Length - 2; i++)
            {
                jointsLength[i] = Vector3.Distance(_legs[leg].Bones[i].position, _legs[leg].Bones[i + 1].position);
            }

            float targetRootDist = Vector3.Distance(copy[0], legTargets[leg].position);
            if (targetRootDist < jointsLength.Sum())
            {

                while (Vector3.Distance(copy[copy.Length - 1], legTargets[leg].position) != 0 || Vector3.Distance(copy[0], _legs[leg].Bones[0].position) != 0)
                {
                    copy[copy.Length - 1] = legTargets[leg].position;

                    for (int i = _legs[leg].Bones.Length - 2; i >= 0; i--)
                    {
                        Vector3 vectorDirector = (copy[i + 1] - copy[i]).normalized;
                        Vector3 movementVector = vectorDirector * jointsLength[i];
                        copy[i] = copy[i + 1] - movementVector;
                    }

                    copy[0] = _legs[leg].Bones[0].position;

                    for (int i = 1; i < _legs[leg].Bones.Length - 1; i++)
                    {
                        Vector3 vectorDirector = (copy[i - 1] - copy[i]).normalized;
                        Vector3 movementVector = vectorDirector * jointsLength[i - 1];
                        copy[i] = copy[i - 1] - movementVector;

                    }
                }

                for (int i = 0; i <= _legs[leg].Bones.Length - 2; i++)
                {
                    Vector3 direction = (copy[i + 1] - copy[i]).normalized;
                    Vector3 antDir = (_legs[leg].Bones[i + 1].position - _legs[leg].Bones[i].position).normalized;
                    Quaternion rot = Quaternion.FromToRotation(antDir, direction);
                    _legs[leg].Bones[i].rotation = rot * _legs[leg].Bones[i].rotation;
                }



                /*
                for(int i = 0; i < _legs.Length; i++)
                {
                    Vector3[] newJointPositions = new Vector3[legTargets.Length];

                    Vector3 P3p; // Goal
                    Vector3 P2;  // Current point
                    Vector3 P2p = new Vector3(0,0,0); // New point

                    //Backwards
                    for (int j = _legs[0].Bones.Length; j > 0; j--)
                    //for (int j = legTargets.Length; j > 0; j--)
                    {


                        if (j == legTargets.Length) //primera iteracion
                        {
                            //fabrik
                            P3p = legFutureBases[i].position; //goal
                            P2 = legTargets[j].position;

                            //        (anterior - nuevo).norm * distancia entre p2 anterior y p3 anterior
                            P2p = ((P2 - P3p).normalized) * Vector3.Distance(_legs[i].EndEffector[0].position, P2);

                        }
                        else
                        {
                            //fabrik
                            P3p = P2p; //el anterior punto que hemos sacado
                            P2 = legTargets[j].position;

                            //        (anterior - nuevo).norm * distancia entre p2 anterior y p3 anterior
                            //P2p = ((P2 - P3p).normalized) * Vector3.Distance(legTargets[j + 1].position, P2);
                            P2p = ((P2 - P3p).normalized) * Vector3.Distance(_legs[i].Bones[j + 1].position, P2);
                        }
                        //newJointPositions[j] = P2p;
                    }

                    //Forwards
                    for (int j = 0; j < legTargets.Length; j++)
                    {
                        if (j == legTargets.Length - 1) //primera iteracion
                        {
                            //fabrik
                            P3p = legFutureBases[i].position; //goal
                            P2 = legTargets[j].position;

                            //        (anterior - nuevo).norm * distancia entre p2 anterior y p3 anterior
                            P2p = ((P2 - P3p).normalized) * Vector3.Distance(_legs[i].EndEffector[0].position, P2);
                        }
                        else
                        {
                            //fabrik
                            P3p = P2p; //el anterior punto que hemos sacado
                            P2 = legTargets[j].position;

                            //        (anterior - nuevo).norm * distancia entre p2 anterior y p3 anterior
                            //P2p = ((P2 - P3p).normalized) * Vector3.Distance(legTargets[j - 1].position, P2);
                            P2p = ((P2 - P3p).normalized) * Vector3.Distance(_legs[i].Bones[j - 1].position, P2);
                        }
                        newJointPositions[j] = P2p;


                    }
                }
                //TODO: hacer for y asignar noewJointPoints a lo que sea que actualiza la pos
                */
            }
            #endregion
        }
    }
}