using DevDays.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class UserServiceTests
{
    UserService userService;
    public UserServiceTests()
    {
        userService = new UserService(null, null, null);
    }

    [Fact]
    public void GreaterThanOrEqualTo5()
    {
        Assert.False(userService.ValidatePassword("abc1"));
        Assert.True(userService.ValidatePassword("abc12"));
    }

    [Fact]
    public void GreaterThanOrEqualTo12()
    {
        Assert.False(userService.ValidatePassword("1234567890abc"));
        Assert.True(userService.ValidatePassword("1234567890ab"));
    }

    [Fact]
    public void NumbersAndLetters()
    {
        Assert.False(userService.ValidatePassword("1234567890"));
        Assert.False(userService.ValidatePassword("abcdefghij"));
        Assert.True(userService.ValidatePassword("Pasword12345"));
    }

    [Fact]
    public void NoRepeatedPatterns()
    {
        Assert.False(userService.ValidateNoRepeatedPatterns("12345aa"));
        Assert.False(userService.ValidateNoRepeatedPatterns("12345abcabc"));
        Assert.True(userService.ValidateNoRepeatedPatterns("12345a54321"));
        Assert.False(userService.ValidateNoRepeatedPatterns("12345aa54321"));
        Assert.False(userService.ValidateNoRepeatedPatterns("1234512345aa"));
        Assert.False(userService.ValidateNoRepeatedPatterns("aa"));
        Assert.False(userService.ValidateNoRepeatedPatterns("6aa"));
    }
}
