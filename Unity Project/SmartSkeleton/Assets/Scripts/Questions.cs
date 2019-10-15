using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable] 
public class Questions {  //a class to hold the variables for each questions


    public int id; //an id number for the quesiton
    public string question; //the text of the question
    public int mCode; //if a muscle is to be shown, the code for the muscles.  The codes are listed in the file "muscle_action_answer_calculator.xlsx"
    public int correct; //the calculated int representing a correct value.  Calculated using "muscle_action_answer_calcualtor.xlsx"
    public int isAlternate; //is there an alternate answer?  1 for yes and 0 for no.
    public int alternate; //the calculated int representing an alternate correct value (i.e. without rotation).  Calculated using "muscle_action_answer_calcualtor.xlsx"
    public string feebackCorrect; //text feedback for correct answers.  Instruction text will automatically be appended to this.
    public string feebackAlternate; //text feedback for alternate correct answers.  Instruction text will be automatically appended to this
    public string feedbackIncorrect; //text feedback for incorrect answers.
    public int pointsCorrect; //points awarded for correct answers
    public int pointsAlternate; //points awarded for alternate correct answers
    public int showMuscleTry;
    public int numberOfTries;
    public int alreadyAnswered;

}
