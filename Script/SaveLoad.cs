using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad {
	public static int SaveCounter;

	public static void addCounter(){
		SaveCounter++;
		BinaryFormatter bf = new BinaryFormatter();
		//Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
		FileStream file = File.Create (Application.persistentDataPath + "/savedCounter.gd"); //you can call it anything you want
		bf.Serialize(file, SaveCounter);
		file.Close();
	}

	public static void loadCounter(){
		if (File.Exists (Application.persistentDataPath + "/savedCounter.gd")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/savedCounter.gd", FileMode.Open);
			SaveCounter = (int)bf.Deserialize (file);
			file.Close ();
		} else {
			SaveCounter = 0;
		}

	
	}

}
