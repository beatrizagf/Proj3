using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;


public class StringPullingPathSmoothing : MonoBehaviour {





    // Use this for initialization
    public static GlobalPath SmoothPath(Vector3 position, GlobalPath currentSolution)
    {
        int i = 0;
        var positions = currentSolution.PathPositions;
        currentSolution.PathPositions.Insert(0, position);
        GlobalPath smoothedPath = new GlobalPath();

        smoothedPath.PathPositions.Add(positions[0]);
        /////////PATHNODES FALTA ADICIONAR
        while (i + 1 < positions.Count)
        {
            int j = i + 1;

            while (j < positions.Count)
            {
                if (j + 1 < positions.Count)
                {
                    var pos_start = positions[i];
                    var pos_next = positions[j + 1];
                    var direction = pos_next - pos_start;
                    float maxLookAhead = (pos_start - pos_next).magnitude;

                    Ray rayVector = new Ray(pos_start, pos_next);
                    RaycastHit hit;
                    if (Physics.Raycast(pos_start, direction, out hit))
                    {
                        if (hit.distance < maxLookAhead)
                        {
                            smoothedPath.PathPositions.Add(positions[j]);
                            //smoothedPath.PathNodes.Add(currentSolution.PathNodes[j]);
                            i = j;
                            break;
                        }
                        else
                        {
                            j++;
                        }
                    }
                    else
                    {
                        j++;
                    }
                }
                else
                {
                    smoothedPath.PathPositions.Add(positions[j]);
                    //smoothedPath.PathNodes.Add(currentSolution.PathNodes[j]);
                    i = j;
                    break;
                }
            }
        }
        return smoothedPath;
    }

    }
