﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//using System.Diagnostics;

public class pixelmanipulated : MonoBehaviour {
	//struct to store pixel point as integer
	public struct intVector2{
		public int x, y;
	}

	public Texture2D originalTexture;
	public Texture2D currentTexture;

	public Color selectedColor;

	//threshold to bug fix color RGB value not equal by small margin
	public float diffThreshold;


	// Use this for initialization
	void Start () {

		//setting temp texture width and height 
		currentTexture = new Texture2D (originalTexture.width, originalTexture.height);
		//fill the new texture with the original one (to avoid "empty" pixels)
		for (int y =0; y<currentTexture.height; y++) {
			for (int x = 0; x<currentTexture.width; x++) {
				if (originalTexture.GetPixel (x, y).grayscale < 0.8f) { //1 = black, 0 = white
					currentTexture.SetPixel (x, y, Color.black);
				} else {
					currentTexture.SetPixel (x, y, Color.white);
				}
			}
		}

		diffThreshold = 0.08f;


		selectedColor = Color.green;
		//print ("texture size: "+currentTexture.width + "x" + currentTexture.height);
		//Apply 
		currentTexture.Apply ();
		//change the object main texture
		this.GetComponent<RawImage>().texture = currentTexture;
	}
	
	public void DebugPoint(BaseEventData data){
		PointerEventData ped = ( PointerEventData )data;
		Vector2 localCursor;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out localCursor))
		return;

		//print("LocalCursor:" + localCursor);

		/*
		 * rumus convert cursor ke dimensi Texture
		 * xTexture = xCursor/widthRecTransform*widthTexture
		 */
		intVector2 cursorPosition;
		cursorPosition.x = (int)(localCursor.x/this.GetComponent<RectTransform> ().sizeDelta.x*currentTexture.width);
		cursorPosition.y = (int)(localCursor.y/this.GetComponent<RectTransform> ().sizeDelta.y*currentTexture.height);


		//Stopwatch stopwatch = Stopwatch.StartNew ();

		floodFill (cursorPosition, currentTexture);
		currentTexture.Apply ();

		//stopwatch.Stop ();

		//print ("Elapse time: "+stopwatch.ElapsedMilliseconds);

	}

	private bool colorEqual(Color me, Color other)
	{
		//method to fix bug where RGB value not equal by small margin
		return Mathf.Abs (me.r - other.r) < diffThreshold && 
			Mathf.Abs (me.g - other.g) < diffThreshold && 
			Mathf.Abs (me.b - other.b) < diffThreshold && 
			Mathf.Abs (me.a - other.a) < diffThreshold;
	}

	private void floodFill(intVector2 startNode, Texture2D texture){
		Queue<intVector2> myQueue = new Queue<intVector2> ();
		myQueue.Enqueue (startNode);

		intVector2 node;
		intVector2 nextNode;

		Color nodeColor;

		while (myQueue.Count>0) {
			node = myQueue.Dequeue ();

			nodeColor = texture.GetPixel (node.x, node.y);
			//print("current Color: "+selectedColor + " | node color: "+nodeColor);


			//fill condition
			if (!colorEqual(nodeColor,Color.black) && 
				!colorEqual(nodeColor,selectedColor) &&
				node.x>=0 && node.y >=0 &&
				node.x<texture.width && node.y<texture.height) {

				//replace color
				texture.SetPixel (node.x, node.y, selectedColor);


				//north
				nextNode.x = node.x;
				nextNode.y = node.y + 1;
				myQueue.Enqueue (nextNode);

				//south
				nextNode.x = node.x;
				nextNode.y = node.y - 1;
				myQueue.Enqueue (nextNode);

				//west
				nextNode.x = node.x - 1;
				nextNode.y = node.y;
				myQueue.Enqueue (nextNode);

				//east
				nextNode.x = node.x + 1;
				nextNode.y = node.y;
				myQueue.Enqueue (nextNode);

			}
		}
	}

	public void clickColorPallete(){
		GameObject palleteObject = EventSystem.current.currentSelectedGameObject;
		selectedColor = palleteObject.GetComponent<Image> ().color;
	}

}