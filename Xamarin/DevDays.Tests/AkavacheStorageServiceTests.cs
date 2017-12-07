using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ReactiveUI;
using ReactiveUI.Testing;
using Microsoft.Reactive.Testing;
using DevDays.Services;
using Splat;
using Akavache;
using DevDays.ViewModels;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using DevDays;
using System.Reactive.Threading.Tasks;
using DevDays.Models;
using Moq;

public class AkavacheStorageServiceTests
{

    UserService getStorageService()
    {
        var akavacheStorage = new AkavacheStorageService(
                    Locator.Current.GetLazyLocator<IFilesystemProvider>(),
                    TaskPoolScheduler.Default,
                    null,
                    new InMemoryBlobCache());

        UserService userService = new UserService(new Lazy<IStorageService>(() => akavacheStorage), null, TaskPoolScheduler.Default);

        return userService;
    }

    [Fact]
    public async Task DataDeleted()
    {
        UserService service = getStorageService();
        UserModel userModel = new UserModel()
        {
            Id = "SomeTestId",
            UserName = "UserName"
        };

        await service.SaveUser(userModel, String.Empty);
        await service.DeleteUser(userModel.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await service.GetUser(userModel.Id));
    }



    [Fact]
    public async Task DataStored()
    {
        UserService service = getStorageService();
        UserModel userModel = new UserModel()
        {
            Id = "SomeTestId",
            UserName = "UserName"
        };

        await service.SaveUser(userModel, String.Empty);
        UserModel storedModel = await service.GetUser(userModel.Id);
        string userId = await service.GetUserId(userModel.UserName);

        Assert.False(Object.ReferenceEquals(userModel, storedModel));
        Assert.Equal("SomeTestId", storedModel.Id);
        Assert.Equal("UserName", storedModel.UserName);
        Assert.Equal(userId, storedModel.Id);
    }



    [Fact]
    public async Task SavePassword()
    {
        string kPassword = "Pasword12345";
        Mock<ISecureStorageService> _storageService = new Mock<ISecureStorageService>();
        _storageService
            .Setup(x => x.SavePassword(It.Is<string>(p => p == "SomeTestId"), It.Is<string>(p => p == kPassword)));

        var akavacheStorage = new AkavacheStorageService(
                    Locator.Current.GetLazyLocator<IFilesystemProvider>(),
                    TaskPoolScheduler.Default,
                    null,
                    new InMemoryBlobCache());

        UserService userService = 
            new UserService(new Lazy<IStorageService>(() => akavacheStorage), 
                new Lazy<ISecureStorageService>(() => _storageService.Object), TaskPoolScheduler.Default);

         
        UserModel userModel = new UserModel()
        {
            Id = "SomeTestId",
            UserName = "UserName"
        };

        await userService.SaveUser(userModel, kPassword);

        _storageService
            .Verify(x => x.SavePassword(It.Is<string>(p => p == userModel.Id), It.Is<string>(p => p == kPassword)),
            Times.Once);
    }

}