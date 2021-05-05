using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionReset : MonoBehaviour
{

    public void clickToReset ()
    {
        QuestionController.Instance.resetQuestions();
    }

}
