
using Auth_System.DTOs;
using Auth_System.Models.Entity;

namespace Auth_System.IService
{
    public interface IAuth
    {
        Task<Tuple<int, User?>> Register(ResgisterReqDto registerReqDto);
        Task<Tuple<int, User?>> Login(LoginReqDto dto);
        Task<int> ForgotPassword(ForgotPasswordReqDto dto);
        Task<int> ResetPassword(ResetPasswordReqDto dto);
        Task<int> VerifyEmail(VerifyEmailReqDto dto);
    }
}

