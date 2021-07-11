using UnityEngine;
using UnityEngine.Events;

public class GyroCamera : MonoBehaviour {

	private Gyroscope gyro;
	private bool gyroSupported;
	private Quaternion rotFix;   //Quaternions are used to represent rotations,Unity internally uses Quaternions to represent all rotations.
								 //Gyroscope in device is right handed but in unity its left handed, that’s why the minus sign is added to the input
	[SerializeField]
	private Transform gameWorld;
	private float startY;

	public UnityEvent OnGyroIsNotSupported;

	void Start () 
	{
		gyroSupported = SystemInfo.supportsGyroscope;
		//Debug.Log("gyroSupported: " + gyroSupported);

		if (gyroSupported) 
		{
			gyro = Input.gyro;
			gyro.enabled = true;

			transform.parent.transform.rotation = Quaternion.Euler(90f, 180f, 0f);
			rotFix = new Quaternion(0f, 0f, 1f, 0f);
		}
		else
		{
			//Your Logic
			OnGyroIsNotSupported.Invoke();
		}
	}

	void Update () 
	{
		if (gyroSupported)
			transform.localRotation = gyro.attitude * rotFix;     //Returns the attitude (ie, orientation in space) of the device.
	}

	void ResetGyroRotation()
	{
		startY = transform.eulerAngles.y;
		gameWorld.rotation = Quaternion.Euler(0f, startY, 0f);
	}
}
