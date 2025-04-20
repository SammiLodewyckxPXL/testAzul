using System.Reflection;

namespace Azul.Core.Tests.Extensions;

public static class TypeExtensions
{
    public static void AssertInterfaceProperty(this Type interfaceType, string propertyName, bool shouldHaveGetter, bool shouldHaveSetter)
    {
        PropertyInfo? property = interfaceType.GetProperty(propertyName);
        Assert.That(property, Is.Not.Null, $"{propertyName} property is missing in the interface");
        if (shouldHaveGetter)
        {
            Assert.That(property!.GetMethod, Is.Not.Null, $"{propertyName} property of the interface does not have a getter");
        }
        else
        {
            Assert.That(property!.GetMethod, Is.Null, $"{propertyName} property of the interface should NOT have a getter");
        }

        if (shouldHaveSetter)
        {
            Assert.That(property.SetMethod, Is.Not.Null, $"{propertyName} property of the interface does not have a setter");
        }
        else
        {
            Assert.That(property.SetMethod, Is.Null, $"{propertyName} property of the interface should NOT have a setter");
        }
    }

    public static void AssertInterfaceMethod(this Type interfaceType, string methodName, Type returnType, params Type[] parameterTypes)
    {
        MethodInfo[] methods = interfaceType.GetMethods().Where(m => m.Name == methodName).ToArray();
        Assert.That(methods.Length, Is.GreaterThan(0), $"{methodName} method is missing in the interface");

        //MethodInfo? method = interfaceType.GetMethod(methodName);
        //Assert.That(method, Is.Not.Null, $"{methodName} method is missing in the interface");

        methods = methods.Where(m => m.ReturnType == returnType).ToArray();
        Assert.That(methods.Length, Is.GreaterThan(0), $"{methodName} method does not have the correct return type in the interface");

        int methodIndex = 0;
        bool matchFound = false;
        while(!matchFound && methodIndex < methods.Length)
        {
            bool isLastMethod = methodIndex == methods.Length - 1;
            MethodInfo method = methods[methodIndex];

            ParameterInfo[] parameters = method.GetParameters();
            matchFound = parameters.Length == parameterTypes.Length;
            if (isLastMethod)
            {
                Assert.That(matchFound, Is.True, $"{methodName} method does not have the correct number of parameters in the interface");
            }

            int parameterIndex = 0;
            while (matchFound && parameterIndex < parameters.Length)
            {
                matchFound = parameters[parameterIndex].ParameterType.FullName!.StartsWith(parameterTypes[parameterIndex].FullName!);
                if (isLastMethod)
                {
                    Assert.That(matchFound, Is.True,
                        $"{methodName} method's parameter at position {parameterIndex} does not have the correct type");
                }
                parameterIndex++;
            }
            methodIndex++;
        }
        Assert.That(matchFound, Is.True, $"{methodName} method does not have the signature in the interface");

    }
}