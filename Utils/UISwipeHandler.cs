using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace SwipeMenu
{
	public class UISwipeHandler : MonoBehaviour
	{
		/// <summary>
		/// If true, swipes will be handled.
		/// </summary>
		public bool handleSwipes = true;

		/// <summary>
		/// Flicks are classed as swipes but with a force greater than SwipeHandler#requiredForceForFlick.
		/// </summary>
		public bool handleFlicks = true;

		/// <summary>
		/// The required force for a swipe to be classes as a flick.
		/// </summary>
		public float requiredForceForFlick = 7f;

		public enum FlickType
		{
			Inertia,
			MoveOne
		}
		/// <summary>
		/// The type of flick. Inertia scrolls kinematically, MoveOne moves the menu in the x direction by one for each flick.
		/// </summary>
		public FlickType flickType = FlickType.Inertia;

		/// <summary>
		/// Once a swipe or flick has finished this will move the menu closest to the centre, to the centre.
		/// </summary>
		public bool lockToClosest = true;

		/// <summary>
		/// Limits the maximum force applied when swiping.
		/// </summary>
		public float maxForce = 15f;

		private Vector3 finalPosition, startpos, endpos, oldpos;
		private float length, startTime, mouseMove, force;
		private bool SW;

		private Vector3 finalPosition_y, startpos_y, endpos_y, oldpos_y;
		private float length_y, startTime_y, mouseMove_y, force_y;

		/// <summary>
		/// Gets a value indicating whether this <see cref="SwipeMenu.SwipeHandler"/> is swiping.
		/// </summary>
		/// <value><c>true</c> if is swiping; otherwise, <c>false</c>.</value>
		public bool isSwiping
		{
			get
			{
				return SW || length != 0;
			}
		}
		void Update()
		{

#if (!UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_WEBGL)
			HandleMobileSwipe ();

#else
			HandleMouseSwipe();
#endif


		}

		private void HandleMobileSwipe()
		{

			if (Input.touchCount > 0)
			{
				
				if (UIManager.Inst.IsBasePanel() == false)
					return;
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					Physics.Raycast(ray, out hit);
					if (hit.collider == null)
						return;
					if (hit.collider.gameObject.CompareTag("LobbySwipe") == false)
						return;


					startTime = Time.time;
					finalPosition = Vector3.zero;
					length = 0;
					SW = false;
					isSwipex = true;
					Vector2 touchDeltaPosition = Input.GetTouch(0).position;
					startpos = new Vector3(touchDeltaPosition.x, 0, touchDeltaPosition.y);
					oldpos = startpos;
				}

				if (Input.GetTouch(0).phase == TouchPhase.Moved)
				{
					if (UIManager.Inst.IsBasePanel() == false)
						return;
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					Physics.Raycast(ray, out hit);
					if (hit.collider == null)
						return;
					if (hit.collider.gameObject.CompareTag("LobbySwipe") == false)
						return;

					Vector2 touchDeltaPosition = Input.GetTouch(0).position;
					Vector3 pos = new Vector3(touchDeltaPosition.x, 0, touchDeltaPosition.y);
					float move_x = Mathf.Abs(oldpos.x - pos.x);
					float move_y = Mathf.Abs(oldpos.z - pos.z);

					if (isSwipex && move_x < move_y)
					{
						isSwipex = false;
					}
					if (isSwipex == false)
						return;

					if (handleSwipes && pos.x != oldpos.x && move_x > move_y)
					{
						var f = pos - oldpos;

						var l = f.x < 0 ? (f.magnitude * Time.deltaTime) : -(f.magnitude * Time.deltaTime);

						l *= .2f;
						if (Mathf.Abs(l) > 0.05f)//0.0035f)
						{
							Menu.instance.Constant(l);
							SW = true;
						}
						
						oldpos = pos;
					}
					
					
				}

				//if (Input.GetTouch(0).phase == TouchPhase.Canceled)
				//{
				//	Debug.Log("Canceled");
				//	SW = false;
				//}

				//if (Input.GetTouch(0).phase == TouchPhase.Stationary)
				//{
				//	Debug.Log("Stationary");
				//	SW = false;
				//}

				if (Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					if (SW && handleFlicks)
					{
						if (UIManager.Inst.IsBasePanel() == false)
							return;
						RaycastHit hit;
						Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
						Physics.Raycast(ray, out hit);
						if (hit.collider == null)
							return;
						if (hit.collider.gameObject.CompareTag("LobbySwipe") == false)
							return;
						Vector2 touchPosition = Input.GetTouch(0).position;
						endpos = new Vector3(touchPosition.x, 0, touchPosition.y);
						finalPosition = endpos - startpos;
						length = finalPosition.x < 0 ? -(finalPosition.magnitude * Time.deltaTime) : (finalPosition.magnitude * Time.deltaTime);

						float move_x = Mathf.Abs(endpos.x - startpos.x);
						float move_y = Mathf.Abs(endpos.z - startpos.z);

						isSwipe = false;
						isSwipex = false;
						if (move_x <= move_y)
						{
							if(SW) Menu.instance.LockToClosest(false);
							return;
						}
						length *= .35f;

						var force = length / (Time.time - startTime);

						force = Mathf.Clamp(force, -maxForce, maxForce);
						Debug.Log("Force : " + Mathf.Abs(force));
						if (handleFlicks && Mathf.Abs(force) > 10f)
						{
							if (flickType == FlickType.Inertia)
							{
								Menu.instance.Inertia(length);
							}
							else
							{
								if (length > 0)
								{
									Menu.instance.MoveLeftRightByAmount(-1);
								}
								else
								{
									Menu.instance.MoveLeftRightByAmount(1);
								}
							}
						}
						else if (lockToClosest && force != 0)
						{
							Menu.instance.LockToClosest(false);
						}
					}

				}

			}



		}

		bool isSwipe = false;
		private void HandleMouseSwipe()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (UIManager.Inst.IsBasePanel() == false)
					return;
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Physics.Raycast(ray, out hit);
				if (hit.collider == null)
					return;
				if (hit.collider.gameObject.CompareTag("LobbySwipe") == false)
					return;
				startTime = Time.time;
				finalPosition = Vector3.zero;

				length = 0;
				Vector2 touchDeltaPosition = Input.mousePosition;
				startpos = new Vector3(touchDeltaPosition.x, 0, touchDeltaPosition.y);
				isSwipe = true;
				isSwipex = true;
				oldpos = startpos;

				SW = false;
			}

			if (isSwipe && Input.GetMouseButtonUp(0))
			{
				if (UIManager.Inst.IsBasePanel() == false)
					return;
				Vector2 touchPosition = Input.mousePosition;
				endpos = new Vector3(touchPosition.x, 0, touchPosition.y);
				finalPosition = endpos - startpos;
				length = finalPosition.x < 0 ? (finalPosition.magnitude * Time.deltaTime) : -(finalPosition.magnitude * Time.deltaTime);

				float move_x = Mathf.Abs(endpos.x - startpos.x);
				float move_y = Mathf.Abs(endpos.z - startpos.z);

				isSwipe = false;
				isSwipex = false;
				if (move_x <= move_y)
				{
					if(SW) Menu.instance.LockToClosest(false);
					return;
				}
				
				length *= .35f;

				force = length / (Time.time - startTime);
				force = Mathf.Clamp(force, -maxForce, maxForce);
				if (handleFlicks && Mathf.Abs(force) > requiredForceForFlick)
				{
					if (flickType == FlickType.Inertia)
					{
						Menu.instance.Inertia(length);
					}
					else
					{
						if (length > 0)
						{
							Menu.instance.MoveLeftRightByAmount(1);
						}
						else
						{
							Menu.instance.MoveLeftRightByAmount(-1);
						}
					}
				}
				else if (lockToClosest && force != 0)
				{
					Menu.instance.LockToClosest(false);
				}
			}

			mouseMove = Helper.GetMouseAxis(MouseAxis.x);
			if (handleSwipes && isSwipe)
			{
				if (UIManager.Inst.IsBasePanel() == false)
					return;
				Vector2 touchDeltaPosition = Input.mousePosition;
				Vector3 pos = new Vector3(touchDeltaPosition.x, 0, touchDeltaPosition.y);
				float move_x = Mathf.Abs(oldpos.x - pos.x);
				float move_y = Mathf.Abs(oldpos.z - pos.z);
				if (isSwipex && move_x < move_y)
				{
					isSwipex = false;
				}
				if (isSwipex == false)
					return;

				if (handleSwipes && pos.x != oldpos.x && move_x > move_y)
				{
					var f = pos - oldpos;

					var l = f.x < 0 ? (f.magnitude * Time.deltaTime) : -(f.magnitude * Time.deltaTime);

					l *= .2f;

					if (Mathf.Abs(l) > 0.8f)
					{
						Menu.instance.Constant(l);
						SW = true;
					}
					oldpos = pos;
				}
			}


		}
		bool isSwipex = false;

	}
}

