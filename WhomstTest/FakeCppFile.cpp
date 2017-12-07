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
#pragma pack(push,1)
struct RTSEnemy
{
    Single Evilness;
    UInt32 Health;
    Single PosX;
    Single PosY;
};
#pragma pack(pop)
//{} HASH: 0BC440A73BCBCC95D55E32C82DF83752

	SOME_KINDOF_API
	/*{{ OutputAndAddFuncSig("void MoveEnemies(RTSEnemy* enemies, UInt32 enemies_count)"); }}*/
	void MoveEnemies(RTSEnemy* enemies, UInt32 enemies_count)
	//{} HASH: 535896183AB5A4A61B7468EDE88CEED8
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
	void ClearDeadEnemies(
		RTSEnemy* enemies, UInt32 enemies_count, UInt32* enemies_count_after)
	//{} HASH: 46F4C1B5F56146B6A1E695D1E627572C
	{
		// code
	}
} // extern "C"