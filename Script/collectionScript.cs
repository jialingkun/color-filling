using UnityEngine;
using System.Collections;

public class CollectionScript {
	public Texture2D tex;
	public int fileID;

	public CollectionScript(Texture2D tex, int fileID){
		this.tex = tex;
		this.fileID = fileID;
	}
}
