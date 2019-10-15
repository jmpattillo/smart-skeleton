using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class QuestionController : MonoBehaviour
{
    
    public static QuestionController Instance;

    public List<string> rawQuestionList = new List<string>(); //holds each individual line of questions.csv
    public List<Questions> theseQuestions = new List<Questions>(); //holds the list of questions.

    public GameObject muscle;
    public MeshRenderer muscleRend;
    public int muscleID;


    public Text text;

    //this set of is just used to display the angles in the development window for debugging.
    public float shoulder_ext_flex;
    public float shoulder_abduct_adduct;
    public float shoulder_rotation;
    public float elbow_ext_flex;
    public float pronate_supinate;
    public float wrist_abduct_adduct;
    public float wrist_ext_flex;
    public float hip_ext_flex;
    public float hip_abduct_adduct;
    public float hip_rotation;
    public float knee_flex;
    public float ankle_flex_ext;
    public float ankle_invert_evert;

    public int debugint;
    public int thisposition;
    public int numberOfQuestions;


    public int shoulderState = 0; //0-3 number of thresholds crossed,  4 is correct, 5 incorrect
    public int elbowState = 0;
    public int wristState = 0;
    public int hipState = 0;
    public int kneeState = 0;
    public int ankleState = 0;

    public bool ignoreWrist;
    public bool ignoreElbow;
    public bool ignoreAnkle;
    public bool ignoreKnee;
    public bool ignoreHip;

    private UDPClient udpClient;

    /*current system for determining joint positions uses a simple threshold angle.  For example,
    depending on the rotation angle, the shoulder would be either be laterally rotated, medially
    rotated, or neutral.  When the user presses the response key, A system of flags are set to determine
    which of the thresholds have been reached and, therefore, which flags are set to 'true'.  The set
    of flags representing a particular set of joint positions comprise the individual bits of a 32 bit 
    integer.  Whether a position is the correct answer to a query can then be determined simply
    by comparing two integers.  The integers representing correct answers for a particular muscle or action can be
    calculated using the spreadsheet "muscle_action_answer_calculator.xlsx"
    */
    [System.Flags] //thanks to Alan Zucconi! https://www.alanzucconi.com/2015/07/26/enum-flags-and-bitwise-operators/
    public enum limbPosition
    {
        neutral = 0, //this is ignored
                     //I read that all these enum lists should have a zero, so neutral is defined as zero.  
        wrist_abduct = 1,
        wrist_adduct = 2, //if wrist is not abducted and not adducted, then it is the anatomical position relative to the coronal plane
        wrist_hyperext = 4, 
        wrist_flexed = 8, //if wrist is not hyperextended or flexed, then it is extended (anatomical position)
        pronated = 16, //if forearm is not pronated, then it is supinated (anatomical position)
        elbow_flexed = 32, //if elbow is not flexed, then it is extended (anatomical position)
        shoulder_m_rotate = 64,
        shoulder_l_rotate = 128, //if shoulder is not m rotated or l rotated, then is in anatomical position 
        shoulder_abduct = 256,
        shoulder_adduct = 512, //not currently used.  
        shoulder_hyperext = 1024,
        shoulder_flex = 2048, //if shoulder is not flexed or hyperextended, then it is extended (anatomical position)
        ankle_inverted = 4096,
        ankle_everted = 8192, //if ankle is not inverted or everted, then it is in anatomical position
        ankle_dorsiflexed = 16384,
        ankle_plantar_flexed = 32768, //if ankle is not dorsiflexed or plantar flexed then it it is in anatomical position
        knee_flexed = 65536, //if knee not flexed then it is extended
        hip_medially_rotated = 131072,
        hip_lateraly_rotated = 262144, //if hip is not medially or laterally rotated, then it is in anatomical position
        hip_abducted = 524288,
        hip_adducted = 1048576, //not currently used
        hip_hyperextended = 2097152,
        hip_flexed = 4194304 //if hip is not hyperextended or flexed, then it is extended (anatomical postion)

        

    }


    public limbPosition capturedPosition = 0;
    public limbPosition currentPosition = 0;


    //TODO  Do I need a more flexible way to determine the intent of the student.  Thresholds may work, especially
    //with visual feedback.
    //TODO Add these thresholds to a preferences scene or seperate tabulated file.  Increase number of thresholds?  
    private float flex_hyperext_threshold = 30f;
    private float shoulder_flex_hyperext_threshold = 20f;
    private float abduct_adduct_threshold = 20f;
    private float wrist_abduct_adduct_threshold = 18f;
    private float rotation_threshold = 20f;
    private float pronation_threshold = 60f;
    private float foot_flexion_threshold = 20f;
    private float invert_evert_threshold = 20f;

    private string ready = "Move skeleton and then press foot pedal or B to enter your answer";
    public bool muscleShowing = false;

    private bool leftShift;

    //TODO implement a more robust and flexible question system.  This set is a demonstration.

    string[] questions = new string[] {
        "Demonstrate the action of the Brachialis",
        "Demonstrate the action of the Pronator Teres",
        "Demonstrate the action of the Triceps Brachia",
        "Demonstrate the action of the Biceps Brachia",
        "Demonstrate flexion of the wrist",
        "Demonstrate hyperextension of the wrist",
        "Demonstrate the action of the costal head of the pectoralis major",
        "Demonstrate external rotation of the arm",
        "Demonstrate the action of the acromial head of the deltoid",
        "Demonstrate the action of the subscapularis",
        "Demonstrate the action of the scapular head of the deltoid",
        "Demonstrate the action of the extensor carpi ulnaris",
        "Demonstrate the action of the clavicular head of the pectoralis major",
        "Demonstrate the action of the teres major",
        "Demnonstrate the action of the latisimus dorsi"};

    //the answers are 32 bit integers whose binary forms represent a set of bit flags as described above

    int[] answers = new int[] {
            32, //brachialis. elbow flexed, all other joints anatomical position (AP)
            48, //pronator teres.  elbow flexed, forearm pronated.  All other joints AP
            512, //tricpes brachia. Shoulder hyperextended.  All other joints AP
            1056,    //Biceps brachia.  Shoulder flexed, elbow fexed.  All others AP
            8,  //flexion of wrist.  Wrist flexed.  All others AP
            4, //hyperextension of the wrist.  Wrist hyperextended all others AP
            64,    //costal pectoralis major.  shoulder medially rotated.  All others AP
            128, //external rotation of the arm.  Shoulder laterally rotated.  All others AP
            256,    //acromial deltoid.  shoulder abducted.  All others AP
            64,  //subscapularis. Shoulder medially rotated.  All others AP
            768,
            6,
            1024,
            512,
            0
            };

            

    public int questionNumber;
    public int questionState;
    public int totalPoints;

    
    void Awake() {Instance = this;}



    // Use this for initialization
    void Start () {
        udpClient = UDPClient.Instance;
        ResponseController.Instance.text.text = ready;
         questionState = 0;
        ignoreWrist = false;
        ignoreElbow = false;
        ignoreAnkle = false;
        ignoreKnee = false;
        ignoreHip = false;
        muscleID = 0;


        // parse question list


        parseQuestions(); 

        //Debug.Log(theseQuestions[1]);
        numberOfQuestions = theseQuestions.Count - 1;
        if (theseQuestions[0].showMuscleTry == 0) { showHideMuscle(theseQuestions[0].mCode); muscleShowing = true; } 


    }
    
    // Update is called once per frame
    void Update ()
    {
        text.text = theseQuestions[questionNumber].question; //set the displayed text to the current question
        currentPosition = 0; //reset the captured position for this frame

        //get joint angles from joint controller scripts
        shoulder_ext_flex = ShoulderJoint.Instance.flex_extend *-1;
        shoulder_abduct_adduct = ShoulderJoint.Instance.abduct_adduct;
        shoulder_rotation = ShoulderJoint.Instance.rotations * -1; 
        elbow_ext_flex = UlnaJoint.Instance.flex_extend;
        pronate_supinate = UlnaJoint.Instance.pronate_supinate;        
        wrist_ext_flex = WristJoint.Instance.flex_extend * -1;
        wrist_abduct_adduct = WristJoint.Instance.abduct_adduct;
        hip_ext_flex = HipJoint.Instance.flex_extend * -1;
        hip_abduct_adduct = HipJoint.Instance.abduct_adduct;
        hip_rotation = HipJoint.Instance.rotations * -1;
        knee_flex = KneeJoint.Instance.flex_extend;
        ankle_flex_ext = AnkleJoint.Instance.flex_extend;
        ankle_invert_evert = AnkleJoint.Instance.invert_evert;

        //set the joint threshold flags 
        currentPosition = capturePosition();
        thisposition = (int)currentPosition;
        //set the state for controlling joint color in the JointColor controller script
        setShoulderState();
        if (!ignoreElbow) { setElbowState(); }
        if (!ignoreWrist) { setWristState(); }
        if (!ignoreHip) { setHipState(); }
        if (!ignoreKnee) { setKneeState(); }
        if (!ignoreAnkle) { setAnkleState(); }

        debugint = (int)currentPosition & 15;
        
        
        
        /*the usb foot pedal sends a 'b' key.  I think using the pedal is a good idea for the final
        product, though a wireless one may work better.  The method of using the other keys 'n' and
        's' is just for the HAPS demo.  A much more robust and feature rich question interface is needed */

        if (Input.GetKeyDown (KeyCode.B)) {
        
            capturedPosition = capturePosition();
            Debug.Log((int)capturedPosition);
            Debug.Log(capturedPosition);
            if ((int)capturedPosition == theseQuestions[questionNumber].correct)
            {
                Debug.Log("Correct");
                questionState = 1;
                ResponseController.Instance.text.text = theseQuestions[questionNumber].feebackCorrect + " Return to anatomical position and press N";
                text.color = Color.green;
                capturedPosition = 0;
                if (theseQuestions[questionNumber].alreadyAnswered == 0)
                {
                    totalPoints += theseQuestions[questionNumber].pointsCorrect;
                    scoreController.Instance.text.text = totalPoints.ToString();
                    theseQuestions[questionNumber].alreadyAnswered = 1;
                }
                //turn joint green?
            }

            else if (((int)capturedPosition == theseQuestions[questionNumber].alternate) && (theseQuestions[questionNumber].isAlternate == 1))
            {
                Debug.Log("Alternate Correct");
                questionState = 1;
                ResponseController.Instance.text.text = theseQuestions[questionNumber].feebackAlternate + " Return to anatomical position and press N";
                text.color = Color.green;
                capturedPosition = 0;
                if (theseQuestions[questionNumber].alreadyAnswered == 0)
                {
                    totalPoints += theseQuestions[questionNumber].pointsAlternate;
                    theseQuestions[questionNumber].alreadyAnswered = 1;
                }
                //t

            }

            else 
            {
                ResponseController.Instance.text.text = theseQuestions[questionNumber].feedbackIncorrect;
                questionState = 2;
                capturedPosition = 0;
                theseQuestions[questionNumber].numberOfTries++;
                if ((theseQuestions[questionNumber].numberOfTries) >= (theseQuestions[questionNumber].showMuscleTry))
                {
                    if (muscleShowing == false)
                    {
                        showHideMuscle(theseQuestions[questionNumber].mCode);
                    }
                    muscleShowing = true;
                }
            }
            //myupperLimb = 0;

        }

        if ((Input.GetKeyDown (KeyCode.N)) && (questionState == 1)) { //next question
                                                                      //TODO add recalibrate here
           // Rotator.Instance.resetCalibration();
            questionState = 0;

            if (muscleShowing) { showHideMuscle(theseQuestions[questionNumber].mCode); muscleShowing = false; }
            ResponseController.Instance.text.text = ready;
            if (questionNumber == numberOfQuestions) {
                questionNumber = 0;
            } else {
                questionNumber++;
            }
            if (theseQuestions[questionNumber].alreadyAnswered == 1)
            {
                text.color = Color.green;
            }
            else
            {
                text.color = Color.white;
            }
            if ((theseQuestions[questionNumber].numberOfTries) >= (theseQuestions[questionNumber].showMuscleTry))
            {
                showHideMuscle(theseQuestions[questionNumber].mCode);
                muscleShowing = true;
            }
            Rotator.Instance.isCalibrated = false; //this will trigger recalibration
            
        }

        
        if (Input.GetKeyDown (KeyCode.S)) { //skip to next question
            questionState = 0;
            if (muscleShowing) { showHideMuscle(theseQuestions[questionNumber].mCode); muscleShowing = false; }
            ResponseController.Instance.text.text = ready;
            if (questionNumber == numberOfQuestions) {
                questionNumber = 0;
            } else {
                questionNumber++;
            }
            if (theseQuestions[questionNumber].alreadyAnswered == 1)
            {
                text.color = Color.green;
            }
            else
            {
                text.color = Color.white;
            }
            if ((theseQuestions[questionNumber].numberOfTries) >= (theseQuestions[questionNumber].showMuscleTry))
            {
                showHideMuscle(theseQuestions[questionNumber].mCode);
                muscleShowing = true;
            }

            Debug.Log(theseQuestions[questionNumber].correct);
            
        }

        if (Input.GetKeyDown(KeyCode.P)) { //go back to previous question
            questionState = 0;
            if (muscleShowing) { showHideMuscle(theseQuestions[questionNumber].mCode); muscleShowing = false; }
            ResponseController.Instance.text.text = ready;
            if (questionNumber == 0) {
                questionNumber = numberOfQuestions;
            } else {
                questionNumber--;
            }
            if (theseQuestions[questionNumber].alreadyAnswered == 1)
            {
                text.color = Color.green;
            }
            else
            {
                text.color = Color.white;
            }
            if ((theseQuestions[questionNumber].numberOfTries) >= (theseQuestions[questionNumber].showMuscleTry))
            {
                showHideMuscle(theseQuestions[questionNumber].mCode);
                muscleShowing = true;
            }
            Debug.Log(theseQuestions[questionNumber].correct);
        }

        if (Input.GetKeyDown(KeyCode.W)) { //ignore the movement of the wrist in calculating the correct position
            ignoreWrist = !ignoreWrist;
            udpClient.SendValue("XdX"); //turn wrist LED off
            if (ignoreWrist) { wristState = 0; }

        }

        if (Input.GetKeyDown(KeyCode.E)) { //ignore the movement of the elbow in calculating the correct position
            ignoreElbow = !ignoreElbow;
            udpClient.SendValue("XhX"); //turn elbow LED off
            if (ignoreElbow) { elbowState = 0; }

        }

        if (Input.GetKeyDown(KeyCode.A))
        { //ignore the movement of the elbow in calculating the correct position
            ignoreAnkle = !ignoreAnkle;
            udpClient.SendValue("XqX"); //turn ankle LED off
            if (ignoreAnkle) { ankleState = 0; }

        }

        if (Input.GetKeyDown(KeyCode.K))
        { //ignore the movement of the elbow in calculating the correct position
            ignoreKnee = !ignoreKnee;
            udpClient.SendValue("XtX"); //turn knee LED off
            if (ignoreKnee) { kneeState = 0; }

        }

        if (Input.GetKeyDown(KeyCode.H))
        { //ignore the movement of the elbow in calculating the correct position
            ignoreHip = !ignoreHip;
            udpClient.SendValue("XyX"); //turn hip LED off
            if (ignoreHip) { hipState = 0; }

        }

        if (Input.GetKeyDown(KeyCode.J)) {

            showHideMuscle(muscleID);
            
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            leftShift = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            leftShift = false;
        }

        if (leftShift && Input.GetKeyDown(KeyCode.R)) 
        {
            resetQuestions();
            if (muscleShowing) { showHideMuscle(theseQuestions[questionNumber].mCode); muscleShowing = false; }
            if ((theseQuestions[questionNumber].numberOfTries) >= (theseQuestions[questionNumber].showMuscleTry))
            {
                showHideMuscle(theseQuestions[questionNumber].mCode);
                muscleShowing = true;
            }
            text.color = Color.white;

            Debug.Log("resetting questions");
        }






        //Debug.Log("updating");
    }


    limbPosition capturePosition () //captures the position of the upper limb and lower limb and returns the set of flags 32 bit integer

    {
        limbPosition myLimb = 0;;
        
        if ((shoulder_ext_flex > 0) && (Mathf.Abs (shoulder_ext_flex) >= shoulder_flex_hyperext_threshold)) {
            myLimb |= limbPosition.shoulder_flex;
         }    
    
        if ((shoulder_ext_flex < 0) && (Mathf.Abs (shoulder_ext_flex) >= shoulder_flex_hyperext_threshold)) {
            myLimb |= limbPosition.shoulder_hyperext;
        }


        if ((shoulder_abduct_adduct > 0) && (Mathf.Abs (shoulder_abduct_adduct) >= abduct_adduct_threshold)) {
            myLimb |= limbPosition.shoulder_abduct;    
        }

        if ((shoulder_rotation > 0) && (Mathf.Abs (shoulder_rotation) >= rotation_threshold)) {
            myLimb |= limbPosition.shoulder_l_rotate;
        }
    
        if ((shoulder_rotation < 0) && (Mathf.Abs (shoulder_rotation) >= rotation_threshold)) {
            myLimb |= limbPosition.shoulder_m_rotate;
        }    


        if (!ignoreElbow)
        {

            if ((elbow_ext_flex > 0) && (Mathf.Abs(elbow_ext_flex) >= flex_hyperext_threshold))
            {
                myLimb |= limbPosition.elbow_flexed;
            }

            if ((pronate_supinate > 0) && (Mathf.Abs(pronate_supinate) >= pronation_threshold))
            {
                myLimb |= limbPosition.pronated;
            }
        }

        if (!ignoreWrist)
        {

            if ((wrist_ext_flex > 0) && (Mathf.Abs(wrist_ext_flex) >= flex_hyperext_threshold))
            {
                myLimb |= limbPosition.wrist_flexed;
            }


            if ((wrist_ext_flex < 0) && (Mathf.Abs(wrist_ext_flex) >= flex_hyperext_threshold))
            {
                myLimb |= limbPosition.wrist_hyperext;
            }


            if ((wrist_abduct_adduct > 0) && (Mathf.Abs(wrist_abduct_adduct) >= wrist_abduct_adduct_threshold))
            {
                myLimb |= limbPosition.wrist_abduct;
            }

            if ((wrist_abduct_adduct < 0) && (Mathf.Abs(wrist_abduct_adduct) >= wrist_abduct_adduct_threshold))
            {
                myLimb |= limbPosition.wrist_adduct;
            }
        }

        if (!ignoreHip)
        {
            if ((hip_ext_flex > 0) && (Mathf.Abs(hip_ext_flex) >= shoulder_flex_hyperext_threshold))
            {
                myLimb |= limbPosition.hip_flexed;
            }

            if ((hip_ext_flex < 0) && (Mathf.Abs(hip_ext_flex) >= shoulder_flex_hyperext_threshold))
            {
                myLimb |= limbPosition.hip_hyperextended;
            }


            if ((hip_abduct_adduct > 0) && (Mathf.Abs(hip_abduct_adduct) >= abduct_adduct_threshold))
            {
                myLimb |= limbPosition.hip_abducted;
            }

            if ((hip_abduct_adduct < 0) && (Mathf.Abs(hip_abduct_adduct) >= abduct_adduct_threshold))
            {
                myLimb |= limbPosition.hip_adducted;
            }

            if ((hip_rotation > 0) && (Mathf.Abs(hip_rotation) >= rotation_threshold))
            {
                myLimb |= limbPosition.hip_lateraly_rotated;
            }

            if ((hip_rotation < 0) && (Mathf.Abs(hip_rotation) >= rotation_threshold))
            {
                myLimb |= limbPosition.hip_medially_rotated;
            }
        }


        if (!ignoreKnee)
        {
            if ((knee_flex > 0) && (Mathf.Abs(knee_flex) >= flex_hyperext_threshold))
            {
                myLimb |= limbPosition.knee_flexed;
            }
            
        }

        if (!ignoreAnkle)
        {
            if ((ankle_flex_ext > 0) && (Mathf.Abs(ankle_flex_ext) >= foot_flexion_threshold))
            {
                myLimb |= limbPosition.ankle_plantar_flexed;
            }


            if ((ankle_flex_ext < 0) && (Mathf.Abs(ankle_flex_ext) >= foot_flexion_threshold))
            {
                myLimb |= limbPosition.ankle_dorsiflexed;
            }


            if ((ankle_invert_evert > 0) && (Mathf.Abs(ankle_invert_evert) >= invert_evert_threshold))
            {
                myLimb |= limbPosition.ankle_everted;
            }

            if ((ankle_invert_evert < 0) && (Mathf.Abs(ankle_invert_evert) >= invert_evert_threshold))
            {
                myLimb |= limbPosition.ankle_inverted;
            }
            
        }
    
        return myLimb;
        

    }

    //the setJointState functions continuously monitor the joint position for crossed thresholds.  The current position byte is
    //added to a byte that will zero out the inputs from the other joints.  if thresholds are crossed then the joint state is updated
    //the joint state is read by other functions to provide feedback that motions have crossed a threshold.

    void setShoulderState () 
    {
        int numthresholds = 0;
        if ((thisposition & 3072) != 0) { numthresholds++; }
        if ((thisposition & 768) != 0) { numthresholds++; }
        if ((thisposition & 192) != 0) { numthresholds++; }
        shoulderState = numthresholds;
    }

    void setElbowState ()
    {
        int numthresholds = 0;
        if ((thisposition & 32) != 0) { numthresholds++; }
        if ((thisposition & 16) != 0) { numthresholds++; }
        elbowState = numthresholds;    
    }

    void setWristState ()
    {
        int numthresholds = 0;
        if ((thisposition & 12) != 0) { numthresholds++; }
        if ((thisposition & 3) != 0) { numthresholds++; }
        wristState = numthresholds;    
    }

    void setHipState ()
    {
        //Debug.Log(thisposition & 6291456);
        int numthresholds = 0;
        if ((thisposition & 6291456) != 0) { numthresholds++; }
        if ((thisposition & 1572864) != 0) { numthresholds++; }
        if ((thisposition & 393216) != 0) { numthresholds++; }
        hipState = numthresholds;
    }

    void setKneeState ()
    {
        int numthresholds = 0;
        if ((thisposition & 65536) != 0) { numthresholds++; }
        kneeState = numthresholds;        
    }

    void setAnkleState ()
    {
        int numthresholds = 0;
        if ((thisposition & 49152) != 0) { numthresholds++; }
        if ((thisposition & 12288) != 0) { numthresholds++; }
        ankleState = numthresholds;
    }



    void parseQuestions () //used for initial parsing of questions.csv.  Also used to reset questions and score.
    {
        ReadFile("questions.csv"); //read the questions file and dump each line in rawQuestionList

        for (int i = 1; i < rawQuestionList.Count; i++) //go through each line of rawQuestionList and parse out the parts of each question
        {

            string[] row = rawQuestionList[i].Split(new char[] { ',' });
            Questions blah = new Questions(); //temporary question to hold each part while parsing
            int.TryParse(row[0], out blah.id);
            blah.question = row[1];
            int.TryParse(row[2], out blah.mCode);
            int.TryParse(row[3], out blah.correct);
            int.TryParse(row[4], out blah.isAlternate);
            int.TryParse(row[5], out blah.alternate);
            blah.feebackCorrect = row[6];
            blah.feebackAlternate = row[7];
            blah.feedbackIncorrect = row[8];
            int.TryParse(row[9], out blah.pointsCorrect);
            int.TryParse(row[10], out blah.pointsAlternate);
            int.TryParse(row[11], out blah.showMuscleTry);
            blah.numberOfTries = 0;
            blah.alreadyAnswered = 0;

            theseQuestions.Add(blah); //add the just parsed question to the list of questions.
            questionNumber = 0; //go to first question
            totalPoints = 0; //zero out score

        } 
    }

    void resetQuestions () //reset all scores and number of tries
    {
        for (int i = 0; i <= numberOfQuestions; i++) {
            theseQuestions[(i)].numberOfTries = 0;
            theseQuestions[(i)].alreadyAnswered = 0;


        }
        totalPoints = 0;
        scoreController.Instance.text.text = totalPoints.ToString();
        
    }


    void ReadFile(string filePath)
    {
        StreamReader questionReader = new StreamReader(filePath);
        while (!questionReader.EndOfStream)
        {
            string line = questionReader.ReadLine();
            rawQuestionList.Add(line);
        }
        questionReader.Close();
    }




    void showHideMuscle ( int thismuscle ) 
    {
        switch ( thismuscle ) 
        {
            case 0:
                break;
            case 1:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_abductor_pollicis_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 2:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_adductor_brevis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 3:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_adductor_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 4:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_adductor_magnus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 5:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_adductor_minimus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 6:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_anconeus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 7:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_biceps_brachii/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 8: //both heads of biceps femoris
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_short_head_biceps_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_long_head_biceps_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 9:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_long_head_biceps_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 10:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_short_head_biceps_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 11:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_brachialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 12:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_brachioradialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 13:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_corocobrachialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 14:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/acromial_part_of_right_deltoid/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 15:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/clavicular_part_of_right_deltoid/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 16:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/spinal_part_of_right_deltoid/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 17: //all heads of deltoid
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/acromial_part_of_right_deltoid/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/clavicular_part_of_right_deltoid/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/spinal_part_of_right_deltoid/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 18:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_carpi_radialis_brevis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 19:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_carpi_radialis_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 20: //both extensor carpi radialis muscles
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_carpi_radialis_brevis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_carpi_radialis_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 21:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_carpi_ulnaris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 22:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_digiti_minimi/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 23:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_digitorum/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 24:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_digitorum_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 25:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_hallicus_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 26:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_indicus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 27:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_pollicis_brevis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 28:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_extensor_pollicis_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 29:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_fibularis_brevis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 30:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_fibularis_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 31:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_fibularis_tertius/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 32:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_flexor_carpi_radialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 33:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_flexor_carpi_ulnaris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 34:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_flexor_digitorum_profundus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 35:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_flexor_digitorum_superficialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 36:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_flexor_hallicus_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 37:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_flexor_pollicis_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 38:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_gemellus_inferior/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 39:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_gemellus_superior/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 40: //must render calcaneal tendon as well as gastroc to show the whole thing
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_gastrocnemius/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_calcaneal_tendon/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 41:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_gluteus_maximus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 42:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_gluteus_medius/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 43:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_gluteus_minimus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 44:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_iliacus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 45: //all hamstrings
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_short_head_biceps_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_long_head_biceps_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_semimembranosis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_semitendonosis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 46: //both iliopsoas muscles
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_iliacus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_psoas_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 47:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_infraspinatus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 48:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_latissimus_dorsi/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 49:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_obterator_externus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 50:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/rright_obterator_internus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 51:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_palmaris_longus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 52:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_pectineus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 53:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/abdominal_part_of_right_pectoralis_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 54:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/clavicular_part_of_right_pectoralis_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 55:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/sternocostal_part_of_right_pectoralis_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 56: //all heads of the pectoralis major
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/abdominal_part_of_right_pectoralis_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/clavicular_part_of_right_pectoralis_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/sternocostal_part_of_right_pectoralis_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 57:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_piriformis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 58:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_plantaris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 59:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_popliteus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 60:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_psoas_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 61:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_pronator_quadratus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 62:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_pronator_teres/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 63:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_quadratus_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 64: //all quadriceps femoris muscles
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_rectus_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_vastus_medialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_vastus_lateralis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_vastus_intermedius/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 65:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_rectus_femoris/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 66:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_sartorius/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 67:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_semimembranosis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 68:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_semitendonosis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 69:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_soleus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_calcaneal_tendon/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 70:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_subscapularis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 71:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_supinator/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 72:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_supraspinatus/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 73: //must render the IT band seperately to show whole muscle connection
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_tensor_fascia_latae/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_iliotibial_band/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 74:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_teres_major/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 75:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_teres_minor/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 76:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_tibialis_anterior/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 77:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_tibialis_posterior/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 78:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_triceps_brachii/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 79:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_vastus_intermedius/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 80:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_vastus_lateralis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            case 81:
                muscle = GameObject.Find("Skeleton/unmoved_structures/muscles/right_vastus_medialis/default");
                muscleRend = muscle.GetComponent<MeshRenderer>();
                muscleRend.enabled = !(muscleRend.enabled);
                break;
            default:
                break;

                
            
        }
        
    }

}
