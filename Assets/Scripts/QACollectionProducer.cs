using UnityEngine;
using System.Collections;

[System.Serializable]
public class Option
{
	public string o;
}

[System.Serializable]
public class QA
{
	public Option[] options;
	public string question;
	public int answerIndex;
}

[System.Serializable]
public class QAcollection
{
	public int nofOptions;
	public QA[] questions;
	public string name;


	public void DebugPrintCollection ()
	{
		Debug.Log ("Collection name: " + name);
		Debug.Log ("Collection nofOptions: " + nofOptions);
		Debug.Log ("Collection number of questions: " + questions.Length);

		for (int i = 0; i < questions.Length; i++) {
			Debug.Log ("Collection Question number " + i + " : " + questions [i].question);

		}


	}
}

public class QACollectionProducer
{

	public enum MathOperation
	{
		Plus,
		Minus,
		Multiply,
		Divide}

	;

	public enum Op2Type
	{
		Normal,
		AllowSwitch,
		Copy,
		CopyNoRandom}

	;

	public QAcollection GetCollection (string indata)
	{
		return JsonUtility.FromJson<QAcollection> (indata);
	}

	public QAcollection ProduceCollection (string QAname, int nofQuestions, int nofOptions, MathOperation operationType, int startRangeMin, int startRangeMax, int modRangeMin, int modRangeMax, Op2Type op2type,
	                                       int maxAnswer = 0, int answerMod = 0, int minAnswer = 0, bool bOrderedAnswers = false, bool bAllowDuplicateQ = false)
	{

		int[] op1used = new int[nofQuestions];
		int[] op2used = new int[nofQuestions];
		int[] options = new int[nofOptions];
		bool isOkTask;
		int answer, answerRange;
		int panicCounter;
		const int MAX_TRIES = 90000;

		Random.InitState ((int)System.DateTime.Now.Ticks);

		QAcollection qac = new QAcollection ();

		startRangeMax++; // add 1 since Random.Range's second argument is exclusive
		modRangeMax++;

		qac.name = QAname;
		qac.nofOptions = nofOptions;
		qac.questions = new QA[nofQuestions];
		for (int i = 0; i < nofQuestions; i++)
			qac.questions [i] = new QA ();


		for (int usedIndex = 0; usedIndex < nofQuestions; usedIndex++) {

			panicCounter = 0;
			do {
				if (op2type == Op2Type.AllowSwitch && Random.Range (0, 100) < 50) {
					op1used [usedIndex] = Random.Range (modRangeMin, modRangeMax);
					op2used [usedIndex] = Random.Range (startRangeMin, startRangeMax);
				} else {
					do {
						op1used [usedIndex] = Random.Range (startRangeMin, startRangeMax);
					} while (operationType == MathOperation.Divide && op1used [usedIndex] % modRangeMin > 0);
					if (op2type == Op2Type.Copy)
						op2used [usedIndex] = op1used [usedIndex] + Random.Range (modRangeMin, modRangeMax);
					else if (op2type == Op2Type.CopyNoRandom) {
						op2used [usedIndex] = op1used [usedIndex];
						if (Random.Range (0, 100) < 50)
							op2used [usedIndex] += modRangeMin;
						else
							op2used [usedIndex] += (modRangeMax - 1);
					} else
						op2used [usedIndex] = Random.Range (modRangeMin, modRangeMax);
				}

				isOkTask = true;

				if (operationType == MathOperation.Minus && op2used [usedIndex] > op1used [usedIndex])
					isOkTask = false;

				if (operationType == MathOperation.Divide && op2used [usedIndex] == 0)
					isOkTask = false;

				for (int i = 0; i < usedIndex; i++) {
					if (op1used [i] == op1used [usedIndex] && op2used [i] == op2used [usedIndex]) {
						isOkTask = false;
						break;
					}
				}
				panicCounter++;
				if (panicCounter > MAX_TRIES) {
					Debug.Log ("Failed to create collection! Too many questions requested for too small a range.");
					return null;
				}
			} while (!isOkTask && bAllowDuplicateQ == false);

			switch (operationType) {
			case MathOperation.Plus:
			default:
				qac.questions [usedIndex].question = op1used [usedIndex] + " + " + op2used [usedIndex];
				answer = op1used [usedIndex] + op2used [usedIndex];
				answerRange = (int)((startRangeMax - 1 + modRangeMax - 1) * 1.5f);
				if (op2type == Op2Type.Copy || op2type == Op2Type.CopyNoRandom)
					answerRange = (int)(((startRangeMax - 1) * 2) * 1.5f);
				break;
			case MathOperation.Minus:
				qac.questions [usedIndex].question = op1used [usedIndex] + " - " + op2used [usedIndex];
				answer = op1used [usedIndex] - op2used [usedIndex];
				answerRange = (int)((startRangeMax - 1) * 1.3f);
				if (op2type == Op2Type.Copy || op2type == Op2Type.CopyNoRandom)
					answerRange = (int)((startRangeMax - 1) * 1.0f);
				break;
			case MathOperation.Multiply:
				qac.questions [usedIndex].question = op1used [usedIndex] + " x " + op2used [usedIndex];
				answer = op1used [usedIndex] * op2used [usedIndex];
				answerRange = (int)(((startRangeMax - 1) * (modRangeMax - 1)) * 1.5f);
				if (op2type == Op2Type.Copy || op2type == Op2Type.CopyNoRandom)
					answerRange = (int)((startRangeMax - 1) * (startRangeMax - 1) * 1.5f);
				break;
			case MathOperation.Divide:
				qac.questions [usedIndex].question = op1used [usedIndex] + " / " + op2used [usedIndex];
				answer = op1used [usedIndex] / op2used [usedIndex];
				answerRange = (startRangeMax) / 3;
				break;
			}

			if (answerRange > maxAnswer && maxAnswer != 0)
				answerRange = maxAnswer + 1;
			
//			if (answerRange <= 0)
//				answerRange = maxAnswer + 1;

			answerRange /= 8;
			if (answerRange < 5)
				answerRange = 5;

			for (int i = 0; i < nofOptions; i++) {
				bool isDuplicate;
				panicCounter = 0;
				do {
					isDuplicate = false;
//					options [i] = Random.Range (0, answerRange) + answerMod;
					options [i] = answer + Random.Range (-answerRange, answerRange); // + answerMod;
					if (options [i] > maxAnswer && maxAnswer != 0)
						options [i] = maxAnswer;
					if (options [i] < minAnswer)
						options [i] = minAnswer;

					if (options [i] == answer)
						isDuplicate = true;
					for (int j = 0; j < i; j++) {
						if (options [i] == options [j]) {
							isDuplicate = true;
							break;
						}
					}
					panicCounter++;
					if (panicCounter > MAX_TRIES) {
						Debug.Log ("Failed to create collection! Not enough possible options for the range.");
						return null;
					}
				} while (isDuplicate);
			}

			qac.questions [usedIndex].answerIndex = Random.Range (0, nofOptions);
			options [qac.questions [usedIndex].answerIndex] = answer;

			if (bOrderedAnswers) {
				System.Array.Sort (options);
				for (int i = 0; i < nofOptions; i++)
					if (options [i] == answer)
						qac.questions [usedIndex].answerIndex = i;
			}

			qac.questions [usedIndex].options = new Option[nofOptions];
			for (int i = 0; i < nofOptions; i++)
				qac.questions [usedIndex].options [i] = new Option ();

			for (int i = 0; i < nofOptions; i++) {
				qac.questions [usedIndex].options [i].o = "" + options [i];
			}

		}

		return qac;
	}


	public QAcollection ProduceCollection_DoubleOp (string QAname, int nofQuestions, int nofOptions, MathOperation operationType, int startRangeMin, int startRangeMax, int modRangeMin, int modRangeMax, int mod2RangeMin, int mod2RangeMax,
	                                                int maxAnswer = 0, int answerMod = 0, int minAnswer = 0, bool bOrderedAnswers = false)
	{

		int[] op1used = new int[nofQuestions];
		int[] op2used = new int[nofQuestions];
		int[] op3used = new int[nofQuestions];
		int[] options = new int[nofOptions];
		bool isOkTask;
		int answer, answerRange;
		int panicCounter;
		const int MAX_TRIES = 10000;

		Random.InitState ((int)System.DateTime.Now.Ticks);

		QAcollection qac = new QAcollection ();

		startRangeMax++; // add 1 since Random.Range's second argument is exclusive
		modRangeMax++;
		mod2RangeMax++;

		qac.name = QAname;
		qac.nofOptions = nofOptions;
		qac.questions = new QA[nofQuestions];
		for (int i = 0; i < nofQuestions; i++)
			qac.questions [i] = new QA ();


		for (int usedIndex = 0; usedIndex < nofQuestions; usedIndex++) {

			panicCounter = 0;
			do {
				do {
					op1used [usedIndex] = Random.Range (startRangeMin, startRangeMax);
				} while (operationType == MathOperation.Divide && op1used [usedIndex] % modRangeMin > 0);
				op2used [usedIndex] = Random.Range (modRangeMin, modRangeMax);
				op3used [usedIndex] = Random.Range (mod2RangeMin, mod2RangeMax);

				isOkTask = true;

				if (operationType == MathOperation.Minus && op2used [usedIndex] > op1used [usedIndex])
					isOkTask = false;

				if (operationType == MathOperation.Divide && op2used [usedIndex] == 0)
					isOkTask = false;

				for (int i = 0; i < usedIndex; i++) {
					if (op1used [i] == op1used [usedIndex] && op2used [i] == op2used [usedIndex] && op3used [i] == op3used [usedIndex]) {
						isOkTask = false;
						break;
					}
				}
				panicCounter++;
				if (panicCounter > MAX_TRIES) {
					Debug.Log ("Failed to create collection! Too many questions requested for too small a range.");
					return null;
				}
			} while (!isOkTask);

			switch (operationType) {
			case MathOperation.Plus:
			default:
				qac.questions [usedIndex].question = op1used [usedIndex] + " + " + op2used [usedIndex] + " + " + op3used [usedIndex];
				answer = op1used [usedIndex] + op2used [usedIndex] + op3used [usedIndex];
				answerRange = (int)((startRangeMax - 1 + modRangeMax - 1) * 1.5f);
				break;
			case MathOperation.Minus:
				qac.questions [usedIndex].question = op1used [usedIndex] + " - " + op2used [usedIndex] + " - " + op3used [usedIndex];
				answer = op1used [usedIndex] - op2used [usedIndex] - op3used [usedIndex];
				answerRange = (int)((startRangeMax - 1) * 1.3f);
				break;
			case MathOperation.Multiply:
				qac.questions [usedIndex].question = op1used [usedIndex] + " x " + op2used [usedIndex] + " x " + op3used [usedIndex];
				answer = op1used [usedIndex] * op2used [usedIndex] * op3used [usedIndex];
				answerRange = (int)(((startRangeMax - 1) * (modRangeMax - 1)) * 1.5f);
				break;
			case MathOperation.Divide:
				qac.questions [usedIndex].question = op1used [usedIndex] + " / " + op2used [usedIndex] + " / " + op3used [usedIndex];
				answer = op1used [usedIndex] / op2used [usedIndex] / op3used [usedIndex];
				answerRange = (startRangeMax);
				break;
			}

			if (answerRange > maxAnswer && maxAnswer != 0)
				answerRange = maxAnswer + 1;

			//			if (answerRange <= 0)
			//				answerRange = maxAnswer + 1;

			answerRange /= 8;
			if (answerRange < 5)
				answerRange = 5;

			for (int i = 0; i < nofOptions; i++) {
				bool isDuplicate;
				panicCounter = 0;
				do {
					isDuplicate = false;
					//					options [i] = Random.Range (0, answerRange) + answerMod;
					options [i] = answer + Random.Range (-answerRange, answerRange); // + answerMod;
					if (options [i] > maxAnswer && maxAnswer != 0)
						options [i] = maxAnswer;
					if (options [i] < minAnswer)
						options [i] = minAnswer;

					if (options [i] == answer)
						isDuplicate = true;
					for (int j = 0; j < i; j++) {
						if (options [i] == options [j]) {
							isDuplicate = true;
							break;
						}
					}
					panicCounter++;
					if (panicCounter > MAX_TRIES) {
						Debug.Log ("Failed to create collection! Not enough possible options for the range.");
						return null;
					}
				} while (isDuplicate);
			}

			qac.questions [usedIndex].answerIndex = Random.Range (0, nofOptions);
			options [qac.questions [usedIndex].answerIndex] = answer;

			if (bOrderedAnswers) {
				System.Array.Sort (options);
				for (int i = 0; i < nofOptions; i++)
					if (options [i] == answer)
						qac.questions [usedIndex].answerIndex = i;
			}

			qac.questions [usedIndex].options = new Option[nofOptions];
			for (int i = 0; i < nofOptions; i++)
				qac.questions [usedIndex].options [i] = new Option ();

			for (int i = 0; i < nofOptions; i++) {
				qac.questions [usedIndex].options [i].o = "" + options [i];
			}

		}

		return qac;
	}


	public QAcollection MergeCollections (QAcollection one, QAcollection two, bool shuffle, string newName = "")
	{
		int nofQuestions = one.questions.Length + two.questions.Length;
		int j = 0;

		if (one.nofOptions != two.nofOptions) {
			Debug.Log ("Cannot join collections with different number of options");
			return null;
		}

		QAcollection qac = new QAcollection ();

		if (newName.Length > 0)
			qac.name = newName;
		else
			qac.name = one.name;
		qac.nofOptions = one.nofOptions;
		qac.questions = new QA[nofQuestions];

		for (int i = 0; i < one.questions.Length; i++, j++)
			qac.questions [j] = one.questions [i];

		for (int i = 0; i < two.questions.Length; i++, j++)
			qac.questions [j] = two.questions [i];


		if (shuffle)
			ShuffleCollection (qac);

		return qac;
	}

	public void ShuffleCollection (QAcollection qac)
	{
		int nofSwitches = qac.questions.Length * 2;
		int first, second;

		if (qac.questions.Length < 2)
			return;

		for (int i = 0; i < nofSwitches; i++) {
			first = Random.Range (0, qac.questions.Length);
			do {
				second = Random.Range (0, qac.questions.Length);
			} while (second == first);

			QA temp = qac.questions [first];
			qac.questions [first] = qac.questions [second];
			qac.questions [second] = temp;
		}
	}

}
