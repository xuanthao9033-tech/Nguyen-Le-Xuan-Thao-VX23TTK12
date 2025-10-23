using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Mappings
{
    public static class AuthExtensions
    {
        public static User ToUserEntity(this RegisterVModel model, int roleId)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "RegisterVModel không được null.");
            }

            return new User
            {
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                UserAddress = model.UserAddress,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                RoleId = roleId,
                FailedLoginCount = 0
            };
        }

        public static RegisterVModel ToRegisterVModel(this User entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "User entity không được null.");
            }

            return new RegisterVModel
            {
                UserName = entity.UserName,
                Email = entity.Email,
                Password = string.Empty,
                Gender = entity.Gender,
                PhoneNumber = entity.PhoneNumber,
                UserAddress = entity.UserAddress
            };
        }

        public static UserVModel ToUserVModel(this User entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "User entity không được null.");
            }

            return new UserVModel
            {
                Id = entity.Id,
                UserName = entity.UserName,
                Email = entity.Email,
                Gender = entity.Gender,
                PhoneNumber = entity.PhoneNumber,
                UserAddress = entity.UserAddress,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                RoleName = entity.Role?.RoleName ?? "Unknown"
            };
        }

        public static User ToUserEntity(this AddUserVModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "AddUserVModel không được null.");
            }

            return new User
            {
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                UserAddress = model.UserAddress,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Admin",
                RoleId = model.RoleId,
                FailedLoginCount = 0
            };
        }

        public static void UpdateUserEntity(this User entity, UpdateUserVModel model)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "User entity không được null.");
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "UpdateUserVModel không được null.");
            }

            entity.UserName = model.UserName;
            entity.Email = model.Email;
            entity.Gender = model.Gender;
            entity.PhoneNumber = model.PhoneNumber;
            entity.UserAddress = model.UserAddress;
            entity.IsActive = model.IsActive ?? entity.IsActive;
            entity.RoleId = model.RoleId;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "Admin";
        }
    }
}
