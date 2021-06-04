using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
public class Grapple : MonoBehaviour
{
    public Animator anime;
    private static Transform player;
    private static readonly float grappleDist = 20f;
    private static readonly float minHeight = 2.2f;
    private static Camera playerCam;

    private Image Crosshair;
    private RectTransform rectCrosshair;
    

    readonly List<grappleInfo> GrappleBuildings = new List<grappleInfo>();
    readonly List<grappleInfo> grappleInfos = new List<grappleInfo>();
    readonly List<Vector3[]> possibleGrapples = new List<Vector3[]>();

    public int rotate = 0;
    public Vector3 finalGrapple;
    public ledgePoint oldGrapple;
    public CharacterController CC;
    

    public float angleDifference(Vector3 a, Vector3 b) // Calculates what angle difference is between Batman facing direction and the moving direction
    {
        a.y = 0f; // Ignore the y camera rotation
        b.y = 0f; // Ignore the y camera rotation
        
        Quaternion cameraToWorldDiff = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(b));
        Vector3 moveTargetDir = cameraToWorldDiff * new Vector3(0,0,1);
        Vector3 sign = Vector3.Cross(moveTargetDir, a);
        float angleDiff = Vector3.Angle(a, moveTargetDir) * (sign.y < 0 ? 1f : -1f);
        
        return angleDiff; // Returns the calculated angle difference
    }

    public static Vector3 ClosestPointsOnTwoLines(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) //Find the closest point of 2 lines
    {
        Vector3 closestPoint = Vector3.zero; //Set the answer point to (0,0,0)

        float a = Vector3.Dot(lineVec1, lineVec1); //Find the dot product of the vector1 direction
        float b = Vector3.Dot(lineVec1, lineVec2); //Find the dot product of the vector1 direction and vector2 direction 
        float e = Vector3.Dot(lineVec2, lineVec2); //Find the dot product of the vector2 direction
 
        float d = a * e - b * b;
        
        if (d == 0.0f) return closestPoint; //lines are parallel
        
        //lines are not parallel
        Vector3 r = linePoint1 - linePoint2;
        float c = Vector3.Dot(lineVec1, r);
        float f = Vector3.Dot(lineVec2, r);
 
        float s = (b * f - c * e) / d;

        if (s <= 1f && s >= 0f) //If the point is on the line segment
            closestPoint = linePoint1 + lineVec1 * s; //Find the closest point

        return closestPoint;
    }	
    
    
    static float[] quadraticFormula(float a, float b, float c) //Quadratic Formula, returns a float array with 2 possible answers
    {
        float[] value = new float[2];
        float tempCalc = Mathf.Pow(b, 2f) - 4f * a * c;

        if (tempCalc < 0f)
        {
            value[0] = -1f;
            value[1] = -1f;
            return value;
        }

        
        
        if (tempCalc > 0f)
        {
            value[0] = (-b + Mathf.Sqrt(Mathf.Pow(b, 2f) - 4f * a * c)) / (2f * a);
            value[1] = (-b - Mathf.Sqrt(Mathf.Pow(b, 2f) - 4f * a * c)) / (2f * a);
            return value;
        }

        else
        {
            value[0] = (-b + Mathf.Sqrt(Mathf.Pow(b, 2f) - 4f * a * c)) / 2f * a;
            value[1] = value[0];
            return value;
        }

    }
    
    
    static Vector3 lineSegmentPointDist(Vector3 a, Vector3 b, Vector3 point, float distance) //Finds a point along the line segment that is certain distance from a point
    {
        float x = Mathf.Pow(b.x - a.x, 2f) + Mathf.Pow(b.y - a.y, 2f) + Mathf.Pow(b.z - a.z, 2f);
        float y = 2f * ((a.x - point.x) * (b.x - a.x) + (a.y - point.y) * (b.y - a.y) + (a.z - point.z) * (b.z - a.z));
        float z = Mathf.Pow(a.x - point.x, 2f) + Mathf.Pow(a.y - point.y, 2f) + Mathf.Pow(a.z - point.z, 2f) - Mathf.Pow(distance, 2f);
        float[] t = quadraticFormula(x, y, z);
        
        if (t[0] >= 0 && t[0] <= 1)
            return a + t[0] * (b - a);
        
        if (t[1] >= 0 && t[1] <= 1)
            return a + t[1] * (b - a);
        
        return Vector3.zero;
    }
    
    
    static bool camVisible(Vector3 targetPoint) //If a point is visible to the camera
    {
        Vector3 viewportPoint = playerCam.WorldToViewportPoint(targetPoint); //Translate the target point to a viewport point
        Ray screenPoint = playerCam.ViewportPointToRay(new Vector3(viewportPoint.x,viewportPoint.y,playerCam.nearClipPlane));
        
        if (viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0) //Check if the point is in the viewport of the camera
        {
            if (!Physics.Linecast(screenPoint.origin,targetPoint + (targetPoint - screenPoint.origin).normalized * -0.2f)) //Cast a ray to see if there is any obstacle in the way of the target point
            {
                return true;
            }
        }
        return false;
    }
    
    bool betweenScreenPointsX(Vector3 point, Vector3 targetPoint1, Vector3 targetPoint2) //Check if a point is between two other point on the x coordinate of the screen
    {
        if (targetPoint1.x <= point.x && point.x <= targetPoint2.x)
            return true;
        
        if (targetPoint2.x <= point.x && point.x <= targetPoint1.x)
            return true;
        
        return false;
    }

    public class ledgePoint //A class for the winner grapple point
    {
        public Vector3[] ledge = new Vector3[2]; //Stores the vertices of the ledge of which the grapple point is on
        public Vector3 grappleSpot; //The winner grapple point

        public ledgePoint() //Initialize the class
        {
            ledge[0] = Vector3.zero;
            ledge[1] = Vector3.zero;
            grappleSpot = Vector3.zero;
        }

        public bool isInRange() //Check if the point is in the possible grapple distant range
        {
            if (Vector3.Distance(player.position,grappleSpot) <= grappleDist)
                return true;

            return false;
        }

        public void renewPoint(bool range) //If the old grapple point is out of range, then try to renew it to a point that is close to it and possible to grapple
        {
            if (!range)
            {
                grappleSpot = lineSegmentPointDist(ledge[0], ledge[1], player.position, grappleDist); //Find the point that satisfy this max distance and along the same ledge
                if (grappleSpot == Vector3.zero) //If there isn't a possible grapple point then set everything to default
                {
                    ledge[0] = Vector3.zero;
                    ledge[1] = Vector3.zero;
                    grappleSpot = Vector3.zero;
                }
                
                if (!camVisible(grappleSpot)) //If the point is not visible to the camera
                {
                    ledge[0] = Vector3.zero;
                    ledge[1] = Vector3.zero;
                    grappleSpot = Vector3.zero;
                }
            }
        }

        public void setValue(Vector3 ledge1, Vector3 ledge2, Vector3 newPoint) //A easy function to change to the ledgePoint data
        {
            ledge[0] = ledge1;
            ledge[1] = ledge2;
            grappleSpot = newPoint;
        }

        public bool isPossible() //Check if the point meets the minimum grapple height
        {
            if (ledge[0] == Vector3.zero && ledge[1] == Vector3.zero && grappleSpot == Vector3.zero)
                return false;

            if (grappleSpot.y - player.position.y < minHeight)
                return false;
            
            return true;
        }
        
        
    }
    private class grappleInfo //Class that stores all the edges of a building that can be grappled
    {
        public readonly GameObject GB; //The game object of the building
        public readonly BoxCollider BC; //The box collider of the building
        public bool visible = false; //The visibility of the building
        public readonly Vector3[] vertices = new Vector3[4]; //The top vertices of the building
        public readonly float[] verticesWorldDist = new float[4]; //The distance of the vertices to the player
        public readonly bool[] verticesVis = new bool[4]; //The visibility of the top vertices

        public grappleInfo(GameObject GB) //Initialize the class object with all the vertex position and the game object and box collider given the building
        {
            this.GB = GB;
            this.BC = GB.GetComponent<BoxCollider>();

            this.vertices[0] = BC.transform.TransformPoint(new Vector3(0.5f,0.5f,0.5f));
            this.vertices[1] = BC.transform.TransformPoint(new Vector3(0.5f,0.5f,-0.5f));
            this.vertices[2] = BC.transform.TransformPoint(new Vector3(-0.5f,0.5f,-0.5f));
            this.vertices[3] = BC.transform.TransformPoint(new Vector3(-0.5f,0.5f,0.5f));
        }

        public bool worldDist() //Check if the closest vertex's distance from the player is in the grapple range
        {
            for (int index = 0; index < 4; index++)
                this.verticesWorldDist[index] = Vector3.Distance(player.position, this.vertices[index]);
            
            if (verticesWorldDist.Min() <= grappleDist)
                return true;
            
            return false;
        }

        public void calcRestInfo(bool rule) //Calculate the rest of the info if the building is in range for grapple
        {
            if (rule)
            {
                for (int index = 0; index < 4; index++)
                {
                    if (camVisible(this.vertices[index]) && this.vertices[index].y - player.position.y >= minHeight)
                    {
                        this.verticesVis[index] = true;
                        if (this.visible == false) this.visible = true;
                    }
                    
                    else
                        this.verticesVis[index] = false;
                    
                }
            }
            
        }
    }

    public void crosshairOnOff() //Function that turn on and off the grapple crosshair if the point is visible and is possible to grapple distance wise and it's not already grappling
    {
        if (oldGrapple.isPossible() && camVisible(oldGrapple.grappleSpot) && anime.GetInteger("Grapple") == 0)
        {
            rectCrosshair.position = playerCam.WorldToScreenPoint(oldGrapple.grappleSpot);
            Crosshair.enabled = true;
        }
        else
            Crosshair.enabled = false;
    }
    
    
    
    void Start()
    {
        playerCam = Camera.main;
        player = this.transform;
        
        Crosshair = GameObject.Find("Screen/Crosshair").GetComponent<Image>();
        rectCrosshair = Crosshair.transform.GetComponent<RectTransform>();
        Crosshair.enabled = false;

        foreach(GameObject building in GameObject.FindGameObjectsWithTag("Grapple")) //Calculate and intialize the buildings with the tag of "Grapple"
            GrappleBuildings.Add(new grappleInfo(building));
        
        oldGrapple = new ledgePoint();
    }


    void Update()
    {
        grappleInfos.Clear(); //Clear the list
        possibleGrapples.Clear(); //Clear the list
        //camPlanes = GeometryUtility.CalculateFrustumPlanes(playerCam);
        foreach (grappleInfo building in GrappleBuildings) //Calculate/Update the visibility of each building
        {
            //if (GeometryUtility.TestPlanesAABB(camPlanes, building.BC.bounds))
            //{
            building.calcRestInfo(building.worldDist()); 
            if (building.visible) grappleInfos.Add(building);
            //}
        }


        foreach (grappleInfo curGI in grappleInfos) //Loop each item in the grappleInfos list
        {
            Vector3[] curVertices = curGI.vertices; //store the current object's vertices to a variable
            bool[] curVerticesVis = curGI.verticesVis; //store the current object's vertex visibility to a variable

            for (int j = 0; j < 4; j++) //Loop the four vertices
            {
                Vector3 curVertex = curVertices[j]; //Store the current vertex
                bool curVertexVis = curVerticesVis[j]; //Store the current vertex visibility
                Vector3 nextVertex = curVertices[(j + 1) % 4]; //Store the next vertex
                bool nextVertexVis = curVerticesVis[(j + 1) % 4]; //Store the next vertex visibility


                if (curVertexVis || nextVertexVis) //If one of the vertex is visible
                {
                    Vector3 screenPosCurVertex = playerCam.WorldToViewportPoint(curVertex); //Translate the world point to viewport point
                    Vector3 screenPosNextVertex = playerCam.WorldToViewportPoint(nextVertex); //Translate the world point to viewport point
                    
                    if (betweenScreenPointsX(new Vector3(0.5f, 0.5f, 0f), screenPosCurVertex, screenPosNextVertex)) //If the camera x is between the two vertex's x
                    {
                        float ledgeSlope = (screenPosNextVertex.y - screenPosCurVertex.y) / (screenPosNextVertex.x - screenPosCurVertex.x); //Use 2D math to find the slope of the ledge
                        float ledgeYIntercept = screenPosCurVertex.y - ledgeSlope * screenPosCurVertex.x; //Use 2D math to find the y-intercept
                        float ledgeX = 0.5f; //Set the x co-ordinate to middle of the screen
                        float ledgeY = ledgeSlope * ledgeX + ledgeYIntercept; //Find the y co-ordinate
                        if (ledgeY > 1f) //If the y co-ordinate is out of the screen then find the point at the highest point of the screen
                        {
                            ledgeY = 1f;
                            ledgeX = (ledgeY - ledgeYIntercept) / ledgeSlope;
                        }

                        Ray distanceRay = playerCam.ViewportPointToRay(new Vector3(ledgeX, ledgeY, playerCam.nearClipPlane)); //Find the ray at the viewport point
                        
                        
                        //LineLineIntersection(curVertex, (nextVertex - curVertex).normalized, distanceRay.origin, distanceRay.direction);

                        //Vector3 correctVector = outPointHit(distanceRay, grappleDist);
                        
                        Vector3 correctVector = ClosestPointsOnTwoLines( curVertex, nextVertex - curVertex, distanceRay.origin,distanceRay.direction); //Find the closest point between the line segment of the two vertex of the building and the direction of the camera's viewport to the grapple point in 2D to find the actual point in 3D
                        
                        if (Physics.Linecast(distanceRay.origin,correctVector - (correctVector - distanceRay.origin).normalized * 0.2f) || correctVector == Vector3.zero) //If there is obstacles between the the player and the potential winner grapple point 
                        {
                            continue; //then continue since the point can't be reached
                        }
                        
                        
                        if (Vector3.Distance(player.position, correctVector) > grappleDist) //If the point is out of range
                        {
                            correctVector = lineSegmentPointDist(curVertex, nextVertex, player.position, grappleDist); //Find the point along the same edge at max distance
                            if (correctVector != Vector3.zero) //If there is a possible point
                            {
                                possibleGrapples.Add(new Vector3[] {curVertex, nextVertex, correctVector}); //Add this point to the possible winner grapple point list
                            }
                        }
                        else //If the point is in range
                        {
                            possibleGrapples.Add(new Vector3[] {curVertex, nextVertex, correctVector}); //Add this point to the possible winner grapple point list
                        }
                    }

                }
            }
        }
    }

    void LateUpdate()
    {
        if (possibleGrapples.Count > 1) //Cycle through all the possible winner grapple point and find the closest one to the center of the screen and that will be the winner grapple point
        {
            float highestDist = Mathf.Infinity;
            
            for (int grapples = 0; grapples < possibleGrapples.Count; grapples++)
            {
                float distance = Vector2.Distance(new Vector2(Screen.width / 2, Screen.height / 2),
                    playerCam.WorldToScreenPoint(possibleGrapples[grapples][2]));

                if (distance < highestDist)
                {
                    oldGrapple.setValue(possibleGrapples[grapples][0],possibleGrapples[grapples][1],possibleGrapples[grapples][2]);
                    highestDist = distance;
                }
                
            }
        }
            
        else if (possibleGrapples.Count == 1) //If there is only one possible winner grapple point then just make that one the winner grapple
            oldGrapple.setValue(possibleGrapples[0][0],possibleGrapples[0][1],possibleGrapples[0][2]);

        else //If there is not possible winner grapple, then use the old winner grapple and try to renew the point to satisfy the grapple point condition again
            oldGrapple.renewPoint(oldGrapple.isInRange());
        
        
        crosshairOnOff(); //Turn the crosshair on or off based on if there is a grapple point on screen or not

        if (rotate != 0) //If rotate is not in default value
        {
            if (!anime.GetBool("Air")) //If Boolean is not in air
                anime.SetInteger("Speed", 0); //Stop the player
            else //If the player is in air
                anime.SetInteger("Speed", 2); //Set the player to gliding
                
            
            Crosshair.enabled = false; //Disable the crosshair
        }

        
        if (Input.GetKeyDown(KeyCode.Space) && Crosshair.enabled && anime.GetInteger("Grapple") == 0 && !anime.GetBool("Air") && camVisible(oldGrapple.grappleSpot)) //If the player wants to grapple
        {
            if (anime.GetBool("Crouching")) anime.SetBool("Crouching", false); //Set crouching to false if Batman is crouching
            finalGrapple = oldGrapple.grappleSpot; //Then store the winner grapple point to the variable finalGrapple
            rotate = 1; //Set rotate mode to 1
        }
        
        else if (Input.GetKeyDown(KeyCode.Space) && Crosshair.enabled && anime.GetInteger("Grapple") == 0 &&
                 anime.GetBool("Air") && camVisible(oldGrapple.grappleSpot)) //If the player wants to grapple and it's in air
        {
            finalGrapple = oldGrapple.grappleSpot; //Then store the winner grapple point to the variable finalGrapple
            rotate = 3; //Set rotate mode to 3
        }
        
        

        
        if (rotate == 1) //If the rotation mode is at 1 then let Batman do the turn animation and set it facing toward the grapple point
        {
            anime.SetInteger ("Speed", 0);
            float TurnAngle = angleDifference(player.forward, (finalGrapple - player.position).normalized);
            
            if (TurnAngle < -135f)
                anime.SetTrigger("Left 180");
            
            else if (TurnAngle < -45 && TurnAngle >= -135f)
                anime.SetTrigger("Left 90");
            
            else if (TurnAngle > 45f && TurnAngle < 135f)
                anime.SetTrigger("Right 90");
            
            else if (TurnAngle > 135f)
                anime.SetTrigger("Right 180");


            else
            {
                anime.ResetTrigger("Left 180");
                anime.ResetTrigger("Left 90");
                anime.ResetTrigger("Right 180");
                anime.ResetTrigger("Right 90");
                
                float rot = Mathf.LerpAngle(0, angleDifference(player.forward,(finalGrapple - player.position).normalized), Time.deltaTime * 10f);
                player.Rotate(0f,rot,0f);

                if (Mathf.Abs(0f - angleDifference(player.forward,
                    (finalGrapple - player.position).normalized)) <= 0.001f)
                    rotate = 2;
                
            }
            

        }

        if (rotate == 2) //If the rotation is finished, then set the Grapple mode to 1
        {
            anime.SetInteger("Grapple",1);
            rotate = 0; //Set rotate mode back to default
        }

        if (rotate == 3) //If Batman is in the air and has to rotate toward to the grapple point
        {
            CC.enabled = false; //Disable the character controller
            float rot = Mathf.LerpAngle(0, angleDifference(player.forward,(finalGrapple - player.position).normalized), Time.deltaTime * 10f); //Calculate the angle needed to turn
            player.Rotate(0f,rot,0f); //Rotate Batman
            playerCam.transform.Rotate(0f,rot,0f); //Rotate Camera
            anime.SetInteger("Speed", 2); //Set it to gliding
            if (Mathf.Abs(0f - angleDifference(player.forward,
                (finalGrapple - player.position).normalized)) <= 0.001f) //If rotation is finished
            {
                anime.SetInteger("Grapple",1); //Set the Grapple mode to 1
                rotate = 0; //Set rotate mode back to default value
            }
            
        }
            
        
    }
}
