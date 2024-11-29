﻿using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MailKit;
using ServerGame.Data;
using ServerGame.Models;
using Microsoft.EntityFrameworkCore;
using ServerGame.DTO;
using ServerGame.ViewModel;
using ServerGame.Service;
using static ServerGame.ViewModel.RatingVM;

namespace ServerGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class APIGameController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        protected ResponseApi _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public APIGameController(ApplicationDbContext db, 
                                 UserManager<ApplicationUser> userManager,
                                 IEmailService emailService,
                                 IConfiguration configuration)
        {
            _db = db;
            _response = new();
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        // GetAllGameLevel
        [HttpGet("GetAllGameLevel")]
        public async Task<IActionResult> GetAllGameLevel()
        {
            try
            {
                var gameLevel = await _db.GameLevels.ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lấy dữ liệu thành công";
                _response.Data = gameLevel;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // GetAllQuestionGame
        [HttpGet("GetAllQuestionGame")]
        public async Task<IActionResult> GetAllQuestionGame()
        {
            try
            {
                var questionGame = await _db.Questions.ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lấy dữ liệu thành công";
                _response.Data = questionGame;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // GetAllRegion
        [HttpGet("GetAllRegion")]
        public async Task<IActionResult> GetAllRegion()
        {
            try
            {
                var region = await _db.Regions.ToListAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lấy dữ liệu thành công";
                _response.Data = region;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                var user = new ApplicationUser
                {
                    Email = registerDTO.Email,
                    Name = registerDTO.Name,
                    UserName = registerDTO.Password,
                    Avatar = registerDTO.LinkAvatar,
                    RegionId = registerDTO.RegionId
                };
                var result = await _userManager.CreateAsync(user, registerDTO.Password);
                if (result.Succeeded)
                {
                    _response.IsSuccess = true;
                    _response.Notification = "Đăng ký thành công";
                    _response.Data = user;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Đăng ký thất bại";
                    _response.Data = result.Errors;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var email = loginRequest.Email;
                var password = loginRequest.Password;
                var user = await _userManager.FindByNameAsync(email);

                if (user != null && await _userManager.CheckPasswordAsync(user, password))
                {
                    _response.IsSuccess = true;
                    _response.Notification = "Đăng nhập thành công";
                    _response.Data = user;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Đăng nhập thất bại";
                    _response.Data = null;
                    return BadRequest(_response);

                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // GetAllQuestionGameByLevel
        [HttpGet("GetAllQuestionGameByLevel/{levelId}")]
        public async Task<IActionResult> GetAllQuestionGameByLevel(int levelId)
        {
            try
            {
                var questionGame = await _db.Questions.Where(x => x.levelId == levelId).ToListAsync(); _response.IsSuccess = true;
                _response.Notification = "Lấy dữ liệu thành công";
                _response.Data = questionGame;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false; _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("SaveResult")]
        public async Task<IActionResult> SaveResult(LevelResultDTO levelResult)
        {
            try
            {
                var levelResultSave = new LevelResult
                {
                    UserId = levelResult.UserId,
                    LevelId = levelResult.LevelId,
                    Score = levelResult.Score,
                    CompletionDate = DateOnly.FromDateTime(DateTime.Now)
                };
                await _db.LevelResults.AddAsync(levelResultSave);
                await _db.SaveChangesAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lưu kết quả thành công";
                _response.Data = levelResult;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Rating
        [HttpGet("Rating/{idRegion}")]
        public async Task<IActionResult> Rating(int idRegion)
        {
            try
            {
                if (idRegion > 0)
                {
                    var nameRegion = await _db.Regions.Where(x => x.RegionId == idRegion).Select(x => x.Name).FirstOrDefaultAsync();
                    if (nameRegion == null)

                    {
                        _response.IsSuccess = false;
                        _response.Notification = "Không tìm thấy khu vực";
                        _response.Data = null;
                        return BadRequest(_response);
                    }
                    var userByRegion = await _db.Users.Where(x => x.RegionId == idRegion).ToListAsync();
                    var resultLevelByRegion = await _db.LevelResults.Where(x => userByRegion.Select(x => x.Id).Contains(x.UserId)).ToListAsync();
                    RatingVM ratingVM = new();
                    ratingVM.NameRegion = nameRegion;
                    ratingVM.userResultSums = new();
                    foreach (var item in userByRegion)
                    {
                        var sumScore = resultLevelByRegion.Where(x => x.UserId == item.Id).Sum(x => x.Score);
                        var sumLevel = resultLevelByRegion.Where(x => x.UserId == item.Id).Count();
                        UserResultSum userResultSum = new();
                        userResultSum.NameUser = item.Name;
                        userResultSum.SumScore = sumScore;
                        userResultSum.SumLevel = sumLevel;
                        ratingVM.userResultSums.Add(userResultSum);
                    }
                    _response.IsSuccess = true;
                    _response.Notification = "Lấy dữ liệu thành công"; _response.Data = ratingVM;
                    return Ok(_response);
                }
                else
                {
                    var user = await _db.Users.ToListAsync();
                    var resultLevel = await _db.LevelResults.ToListAsync();
                    string nameRegion = "Tất cả";
                    RatingVM ratingVM = new();
                    ratingVM.NameRegion = nameRegion; ratingVM.userResultSums = new();
                    foreach (var item in user)
                    {
                        var sumScore = resultLevel.Where(x => x.UserId == item.Id).Sum(x => x.Score); var sumLevel = resultLevel.Where(x => x.UserId == item.Id).Count();
                        UserResultSum userResultSum = new();
                        userResultSum.NameUser = item.Name; userResultSum.SumScore = sumScore;
                        userResultSum.SumLevel = sumLevel;
                        ratingVM.userResultSums.Add(userResultSum);

                    }
                    _response.IsSuccess = true;
                    _response.Notification = "Lấy dữ liệu thành công";
                    _response.Data = ratingVM;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }
        // Get User Information
        [HttpGet("GetUserInformation/{userId}")]
        public async Task<IActionResult> GetUserInformation(string userId)
        {
            try
            {
                var user = await _db.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;
                    return BadRequest(_response);
                }
                UserInformationVM userInformationVM = new();
                userInformationVM.Name = user.Name;
                userInformationVM.Email = user.Email;
                userInformationVM.avatar = user.Avatar;
                userInformationVM.Region = await _db.Regions.Where(x => x.RegionId == user.RegionId)
                .Select(x => x.Name).FirstOrDefaultAsync();
                _response.IsSuccess = true;
                _response.Notification = "Lấy dữ liệu thành công";
                _response.Data = userInformationVM;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lôi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Change User Password
        [HttpPut("ChangeUserPassword")]
        public async Task<IActionResult> ChangeUserPassword(ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var user = await _db.Users.Where(x => x.Id == changePasswordDTO.UserId).FirstOrDefaultAsync();
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;
                    return BadRequest(_response);
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDTO.OldPassword, changePasswordDTO.NewPassword);
                if (result.Succeeded)
                {
                    _response.IsSuccess = true;
                    _response.Notification = "Đổi mật khẩu thành công";
                    _response.Data = "";
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Đổi mật khẩu thất bại";
                    _response.Data = result.Errors;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Update User Information
        [HttpPut("UpdateUserInformation")]
        public async Task<IActionResult> UpdateUserInformation([FromForm] UpdateUserInformationDTO userInformationDTO)
        {
            try
            {
                var user = await _db.Users.Where(x => x.Id == userInformationDTO.UserId).FirstOrDefaultAsync();
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;
                    return BadRequest(_response);
                }
                user.Name = userInformationDTO.Name;
                user.RegionId = userInformationDTO.RegionId;
                if (userInformationDTO.Avatar != null)
                {
                    var fileExtension = Path.GetExtension(userInformationDTO.Avatar.FileName);
                    var fileName = $"{userInformationDTO.UserId}{fileExtension}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await userInformationDTO.Avatar.CopyToAsync(stream);
                    }
                    user.Avatar = fileName;
                }
                await _db.SaveChangesAsync(); _response.IsSuccess = true;
                _response.Notification = "Cập nhật thông tin thành công";
                _response.Data = user;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Delete Account
        [HttpDelete("DeleteAccount/{userId}")]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            try
            {
                var user = await _db.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;
                    return BadRequest(_response);
                }

                user.IsDeleted = true;
                await _db.SaveChangesAsync();
                _response.IsSuccess = true;
                _response.Notification = "Xóa người dùng thành công";
                _response.Data = user;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Forgot Password
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;

                    return BadRequest(_response);
                }
                Random random = new();
                string OTP = random.Next(100000, 999999).ToString();
                user.OTP = OTP;
                await _userManager.UpdateAsync(user);
                await _db.SaveChangesAsync();
                string subject = "Reset Password Game 106 - " + Email;
                string message = "Mã OTP của bạn là: " + OTP;
                await _emailService.SendEmailAsync(Email, subject, message);
                _response.IsSuccess = true;
                _response.Notification = "Gửi mã OTP thành công";
                _response.Data = "email sent to " + Email;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Check OTP
        [HttpPost("CheckOTP")]
        public async Task<IActionResult> CheckOTP(CheckOTPDTO checkOTPDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(checkOTPDTO.Email);
                if (user == null)
                {

                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;

                    return BadRequest(_response);
                }

                var stringOTP = Convert.ToInt32(checkOTPDTO.OTP).ToString();
                if (user.OTP == stringOTP)
                {
                    _response.IsSuccess = true;
                    _response.Notification = "Mã OTP chính xác";
                    _response.Data = user.Email;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Mã OTP không chính xác";
                    _response.Data = null;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        // Reset Password
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Không tìm thấy người dùng";
                    _response.Data = null;
                    return BadRequest(_response);
                }
                var stringOTP = Convert.ToInt32(resetPasswordDTO.OTP).ToString();
                if (user.OTP == stringOTP)
                {
                    DateTime now = DateTime.Now;
                    user.OTP = $"{stringOTP}_used_" + now.ToString("yyyy_MM_dd_HH_mm_ss");
                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    user.PasswordHash = passwordHasher.HashPassword(user, resetPasswordDTO.NewPassword);
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        _response.IsSuccess = true;
                        _response.Notification = "Đổi mật khẩu thành công";
                        _response.Data = resetPasswordDTO.Email;
                        return Ok(_response);
                    }
                    else
                    {
                        _response.IsSuccess = false;
                        _response.Notification = "Đổi mật khẩu thất bại";
                        _response.Data = result.Errors;
                        return BadRequest(_response);
                    }
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Notification = "Mã OTP không chính xác";
                    _response.Data = null;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Notification = "Lỗi";
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }
    }
}