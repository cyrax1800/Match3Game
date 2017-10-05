using System;
using System.Collections.Generic;

[Serializable]
public class TestCase
{
    public List<Test> testCase;
}

[Serializable]
public class Test
{
    public int no;
    public List<int> board;
    public List<int> target;
    public string description;
}