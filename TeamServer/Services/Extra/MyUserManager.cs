using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HardHatCore.TeamServer.Services.Extra
{
    public class MyUserManager : UserManager<UserInfo>
    {
        MyPasswordHasher _MyPasswordHasher = new();

        public MyUserManager(IUserStore<UserInfo> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<UserInfo> passwordHasher, IEnumerable<IUserValidator<UserInfo>> userValidators, IEnumerable<IPasswordValidator<UserInfo>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<UserInfo>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        //override the default password validation
        public override async Task<bool> CheckPasswordAsync(UserInfo user, string password)
        {
            ThrowIfDisposed();
            var passwordStore = GetPasswordStore();
            if (user == null)
            {
                return false;
            }

            var result = await VerifyPasswordAsync(passwordStore, user, password).ConfigureAwait(false);
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                await UpdatePasswordHash(user, password,false).ConfigureAwait(false);
                await UpdateUserAsync(user).ConfigureAwait(false);
            }

            var success = result != PasswordVerificationResult.Failed;
            if (!success)
            {
                Console.WriteLine("Password verification failed, invalid password");
            }
            return success;
        }

        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<UserInfo> store, UserInfo user, string password)
        {
            var hash = await store.GetPasswordHashAsync(user, CancellationToken).ConfigureAwait(false);
            if (hash == null)
            {
                return PasswordVerificationResult.Failed;
            }
            return _MyPasswordHasher.VerifyHashedPassword(user, hash, password);
        }

        private IUserPasswordStore<UserInfo> GetPasswordStore()
        {
            var cast = Store as IUserPasswordStore<UserInfo>;
            if (cast == null)
            {
                Console.WriteLine("Store is not a IUserPasswordStore please update the UserStore Class");
            }
            return cast;
        }
    }
}
