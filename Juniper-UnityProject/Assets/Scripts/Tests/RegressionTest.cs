#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class RegressionTest
{
    [UnityTest]
    public IEnumerator RunRegressionTest()
    {
        bool? success = null;

      
        yield return new WaitForSeconds(1);

        if(success == null) {
            Assert.Fail("success was not set from test");
        }
        
        Assert.IsTrue(success);
    }
}
#endif
