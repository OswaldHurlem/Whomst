typedef uint32_t  UInt32;
typedef float     Single;

extern "C" {
SOMETHING_API
/*{{
	void OutputCppStruct<T>() where T:struct
	{
		var type = typeof(T);
		WhomstOut.WriteLine("#pragma pack(push,1)");
		WhomstOut.WriteLine($"struct {type.Name}");
		WhomstOut.WriteLine("{");
	
		foreach (var f in type.GetFields())
		{
			WhomstOut.WriteLine($"    {f.FieldType.Name} {f.Name};");
		}
	
		WhomstOut.WriteLine("};");
		WhomstOut.WriteLine("#pragma pack(pop)");
	}
	
	var funcSigs = new List<string>();
	void OutputAndAddFuncSig(string fs)
	{
		WhomstOut.WriteLine(fs);
		funcSigs.Add(fs);
	}
	
	OutputCppStruct<RTSEnemy>();
}}*/
//{}

	SOME_KINDOF_API
	/*{{ OutputAndAddFuncSig("void MoveEnemies(RTSEnemy* enemies, UInt32 enemies_count, float deltaTime)"); }}*/
	//{}
	{
		// code
	}

	//!! AtString is a convenience function for correctly outputting multi-line strings
	SOME_KINDOF_API
	/*{{
	OutputAndAddFuncSig(AtString(@"
	void ClearDeadEnemies(
		RTSEnemy* enemies, UInt32 enemies_count, UInt32* enemies_count_after)
	"));
	}}*/
	//{}
	{
		// code
	}
} // extern "C"