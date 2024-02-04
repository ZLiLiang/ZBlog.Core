using Microsoft.Extensions.Logging;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class WeChatConfigService : BaseService<WeChatConfig>, IWeChatConfigService
    {
        readonly IUnitOfWorkManage _unitOfWorkManage;
        readonly ILogger<WeChatConfigService> _logger;
        public WeChatConfigService(IUnitOfWorkManage unitOfWorkManage, ILogger<WeChatConfigService> logger)
        {
            this._unitOfWorkManage = unitOfWorkManage;
            this._logger = logger;
        }

        public async Task<MessageModel<WeChatApiDto>> GetToken(string publicAccount)
        {
            var config = await base.QueryById(publicAccount);
            if (config == null)
                MessageModel<string>.SUCCESS($"公众号{publicAccount}未维护至系统");  //还没过期,直接返回 
            if (config.TokenExpiration > DateTime.Now)
            {
                //再次判断token在微信服务器是否正确
                var wechatIP = await WeChatHelper.GetWechatIP(config.Token);
                if (wechatIP.Errcode == 0)
                    MessageModel<WeChatApiDto>.SUCCESS("", new WeChatApiDto { Access_Token = config.Token });//还没过期,直接返回
            }

            //过期了,重新获取
            var data = await WeChatHelper.GetToken(config.Appid, config.Appsecret);
            if (data.Errcode.Equals(0))
            {
                config.Token = data.Access_Token;
                config.TokenExpiration = DateTime.Now.AddSeconds(data.Expires_In);
                await base.Update(config);

                return MessageModel<WeChatApiDto>.SUCCESS("", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.FAIL($"\r\n获取Token失败\r\n错误代码:{data.Errcode}\r\n错误信息:{data.Errmsg}");
            }
        }

        public async Task<MessageModel<WeChatApiDto>> RefreshToken(string publicAccount)
        {
            var config = await this.QueryById(publicAccount);
            if (config == null) MessageModel<string>.SUCCESS($"公众号{publicAccount}未维护至系统");//还没过期,直接返回  
            //过期了,重新获取
            var data = await WeChatHelper.GetToken(config.Appid, config.Appsecret);
            if (data.Errcode.Equals(0))
            {
                config.Token = data.Access_Token;
                config.TokenExpiration = DateTime.Now.AddSeconds(data.Expires_In);
                await this.Update(config);
                return MessageModel<WeChatApiDto>.SUCCESS("", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.FAIL($"\r\n获取Token失败\r\n错误代码:{data.Errcode}\r\n错误信息:{data.Errmsg}");
            }
        }

        public async Task<MessageModel<WeChatApiDto>> GetTemplate(string id)
        {
            var res = await GetToken(id);
            if (!res.Success)
                return res;
            var data = await WeChatHelper.GetTemplate(res.Response.Access_Token);
            if (data.Errcode.Equals(0))
                return MessageModel<WeChatApiDto>.SUCCESS("", data);
            else
                return MessageModel<WeChatApiDto>.SUCCESS($"\r\n获取模板失败\r\n错误代码:{data.Errcode}\r\n错误信息:{data.Errmsg}", data);
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatApiDto>> GetMenu(string id)
        {
            var res = await GetToken(id);
            if (!res.Success) return res;
            var data = await WeChatHelper.GetMenu(res.Response.Access_Token);
            if (data.Errcode.Equals(0))
            {
                return MessageModel<WeChatApiDto>.SUCCESS("", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.SUCCESS($"\r\n获取菜单失败\r\n错误代码:{data.Errcode}\r\n错误信息:{data.Errmsg}", data);
            }
        }

        public async Task<MessageModel<WeChatApiDto>> GetSubUser(string id, string openid)
        {
            var res = await GetToken(id);
            if (!res.Success) return res;
            var data = await WeChatHelper.GetUserInfo(res.Response.Access_Token, openid);
            if (data.Errcode.Equals(0))
            {
                return MessageModel<WeChatApiDto>.SUCCESS("", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.SUCCESS($"\r\n获取订阅用户失败\r\n错误代码:{data.Errcode}\r\n错误信息:{data.Errmsg}", data);
            }
        }

        public async Task<MessageModel<WeChatApiDto>> GetSubUsers(string id)
        {
            var res = await GetToken(id);
            if (!res.Success)
                return res;
            var data = await WeChatHelper.GetUsers(res.Response.Access_Token);
            if (data.Errcode.Equals(0))
            {
                data.Users = new List<WeChatApiDto>();
                foreach (var openId in data.Data.Openid)
                {
                    data.Users.Add(await WeChatHelper.GetUserInfo(res.Response.Access_Token, openId));
                }

                return MessageModel<WeChatApiDto>.SUCCESS("", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.SUCCESS($"\r\n获取订阅用户失败\r\n错误代码:{data.Errcode}\r\n错误信息:{data.Errmsg}", data);
            }
        }

        public async Task<string> Valid(WeChatValidDto validDto, string body)
        {
            WeChatXMLDto weChatData = null;
            string objReturn = null;
            try
            {
                _logger.LogInformation("会话开始");
                if (string.IsNullOrEmpty(validDto.PublicAccount))
                    throw new Exception("没有微信公众号唯一标识id数据");
                var config = await base.QueryById(validDto.PublicAccount);
                if (config == null)
                    throw new Exception($"公众号不存在=>{validDto.PublicAccount}");

                _logger.LogInformation(JsonHelper.GetJson<WeChatValidDto>(validDto));
                var token = config.InteractiveToken;    //验证用的token 和access_token不一样
                string[] arrTmp = { token, validDto.Timestamp, validDto.Nonce };
                Array.Sort(arrTmp);
                string combineString = string.Join("", arrTmp);
                string encryption = MD5Helper.SHA1Encrypt(combineString).ToLower();

                _logger.LogInformation(
                    $"来自公众号:{validDto.PublicAccount}\r\n" +
                    $"微信signature:{validDto.Signature}\r\n" +
                    $"微信timestamp:{validDto.Timestamp}\r\n" +
                    $"微信nonce:{validDto.Nonce}\r\n" +
                    $"合并字符串:{combineString}\r\n" +
                    $"微信服务器signature:{validDto.Signature}\r\n" +
                    $"本地服务器signature:{encryption}"
                );

                if (encryption == validDto.Signature)
                {
                    //判断是首次验证还是交互?
                    if (string.IsNullOrEmpty(validDto.EchoStr))
                    {
                        //非首次验证 
                        weChatData = XmlHelper.ParseFormByXml<WeChatXMLDto>(body, "xml");
                        weChatData.PublicAccount = validDto.PublicAccount;
                        objReturn = await HandleWeChat(weChatData);
                    }
                    else
                    {
                        //首次接口地址验证 
                        objReturn = validDto.EchoStr;
                    }
                }
                else
                {
                    objReturn = "签名验证失败";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"会话出错(信息)=>\r\n{ex.Message}");
                _logger.LogInformation($"会话出错(堆栈)=>\r\n{ex.StackTrace}");
                //返回错误给用户 
                objReturn = string.Format(@$"<xml><ToUserName><![CDATA[{weChatData?.FromUserName}]]></ToUserName>
                                                    <FromUserName><![CDATA[{weChatData?.ToUserName}]]></FromUserName>
                                                    <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                                    <MsgType><![CDATA[text]]></MsgType>
                                                    <Content><![CDATA[{ex.Message}]]></Content></xml>");
            }
            finally
            {
                _logger.LogInformation($"微信get数据=>\r\n{JsonHelper.GetJson<WeChatValidDto>(validDto)}");
                _logger.LogInformation($"微信post数据=>\r\n{body}");
                _logger.LogInformation($"返回微信数据=>\r\n{objReturn}");
                _logger.LogInformation($"会话结束");
            }
            return objReturn;
        }

        public async Task<MessageModel<WeChatResponseUserInfo>> GetQRBind(WeChatUserInfo info)
        {
            var res = await GetToken(info?.Id);
            if (!res.Success)
                return MessageModel<WeChatResponseUserInfo>.FAIL(res.Msg);
            var push = new WeChatQRDto
            {
                Expire_Seconds = 604800,
                Action_Name = "QR_STR_SCENE",
                Action_Info = new WeChatQRActionDto
                {
                    Scene = new WeChatQRActionInfoDto
                    {
                        Scene_Str = $"bind_{info?.Id}"
                    }
                }
            };
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo
            {
                CompanyCode = info.CompanyCode,
                Id = info.Id
            };

            var pushJson = JsonHelper.GetJson<WeChatQRDto>(push);
            var data = await WeChatHelper.GetQRCode(res.Response.Access_Token, pushJson);
            WeChatQR weChatQR = new WeChatQR
            {
                BindCompanyId = info.CompanyCode,
                BindJobId = info.UserID,
                BindJobNick = info.UserNick,
                CrateTime = DateTime.Now,
                PublicAccount = info.Id,
                Ticket = data.Ticket
            };
            data.Id = info.UserID;
            await this.BaseDal.Db.Insertable(weChatQR)
                .ExecuteCommandAsync();
            reData.UsersData = data;

            return MessageModel<WeChatResponseUserInfo>.SUCCESS("获取二维码成功", reData);
        }

        public async Task<MessageModel<WeChatResponseUserInfo>> PushCardMsg(WeChatCardMsgDataDto msg, string ip)
        {
            var bindUser = await base.BaseDal.Db.Queryable<WeChatSub>()
                .Where(it => it.SubFromPublicAccount == msg.Info.Id && it.CompanyId == msg.Info.CompanyCode && it.IsUnBind == false && msg.Info.UserID.Contains(it.SubJobId))
                .SingleAsync();
            if (bindUser == null)
                return MessageModel<WeChatResponseUserInfo>.FAIL("用户不存在或者已经解绑!");
            var res = await GetToken(msg?.Info?.Id);
            if (!res.Success)
                return MessageModel<WeChatResponseUserInfo>.FAIL(res.Msg);
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo
            {
                CompanyCode = msg.Info.CompanyCode,
                Id = msg.Info.Id
            };

            try
            {
                var pushData = new WeChatPushCardMsgDto
                {
                    Template_Id = msg.CardMsg.Template_Id,
                    Url = msg.CardMsg.Url,
                    Touser = bindUser.SubUserOpenID,
                    Data = new WeChatPushCardMsgDetailDto
                    {
                        First = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.First,
                            Color = msg.CardMsg.Color1
                        },
                        Keyword1 = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.Keyword1,
                            Color = msg.CardMsg.Color1
                        },
                        Keyword2 = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.Keyword2,
                            Color = msg.CardMsg.Color2
                        },
                        Keyword3 = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.Keyword3,
                            Color = msg.CardMsg.Color3
                        },
                        Keyword4 = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.Keyword4,
                            Color = msg.CardMsg.Color4
                        },
                        Keyword5 = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.Keyword5,
                            Color = msg.CardMsg.Color5
                        },
                        Remark = new WeChatPushCardMsgValueColorDto
                        {
                            Value = msg.CardMsg.Remark,
                            Color = msg.CardMsg.ColorRemark
                        }
                    }
                };
                var pushJson = JsonHelper.GetJson<WeChatPushCardMsgDto>(pushData);
                var data = await WeChatHelper.SendCardMsg(res.Response.Access_Token, pushJson);
                reData.UsersData = data;

                try
                {
                    var pushLog = new WeChatPushLog
                    {
                        PushLogCompanyId = msg.Info.CompanyCode,
                        PushLogPublicAccount = msg.Info.Id,
                        PushLogContent = pushJson,
                        PushLogOpenId = bindUser.SubUserOpenID,
                        PushLogToUserId = bindUser.SubJobId,
                        PushLogStatus = data.Errcode == 0 ? "Y" : "N",
                        PushLogRemark = data.Errmsg,
                        PushLogTime = DateTime.Now,
                        PushLogTemplateId = msg.CardMsg.Template_Id,
                        PushLogIp = ip
                    };
                    await base.BaseDal.Db.Insertable(pushLog)
                        .ExecuteCommandAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"记录失败\r\n{ex.Message}\r\n{ex.StackTrace}");
                }

                if (reData.UsersData.Errcode.Equals(0))
                {
                    return MessageModel<WeChatResponseUserInfo>.SUCCESS("卡片消息推送成功", reData);
                }
                else
                {
                    return MessageModel<WeChatResponseUserInfo>.SUCCESS("卡片消息推送失败", reData);
                }
            }
            catch (Exception ex)
            {
                return MessageModel<WeChatResponseUserInfo>.SUCCESS($"卡片消息推送错误=>{ex.Message}", reData);
            }
        }

        public async Task<MessageModel<WeChatApiDto>> PushTxtMsg(WeChatPushTestDto msg)
        {
            var res = await GetToken(msg.SelectWeChat);
            if (!res.Success)
                return res;
            var token = res.Response.Access_Token;
            if (msg.SelectBindOrSub.Equals("sub"))
            {
                return await PushText(token, msg);
            }
            else
            {
                MessageModel<WeChatApiDto> messageModel = new MessageModel<WeChatApiDto>
                {
                    Success = true
                };
                //绑定用户
                if (msg.SelectOperate.Equals("one"))
                {
                    //发送单个 
                    var usrs = base.BaseDal.Db.Queryable<WeChatSub>()
                        .Where(it => it.SubFromPublicAccount.Equals(msg.SelectWeChat) && it.CompanyId.Equals(msg.SelectCompany) && it.SubJobId.Equals(msg.SelectUser))
                        .ToList();
                    foreach (var item in usrs)
                    {
                        msg.SelectUser = item.SubUserOpenID;
                        var info = await PushText(token, msg);
                        if (!info.Success)
                            messageModel.Success = false;
                        messageModel.Msg += info.Msg;
                    }
                }
                else
                {
                    //发送所有
                    var usrs = BaseDal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount.Equals(msg.SelectWeChat) && t.CompanyId.Equals(msg.SelectCompany)).ToList();
                    foreach (var item in usrs)
                    {
                        msg.SelectUser = item.SubUserOpenID;
                        var info = await PushText(token, msg);
                        if (!info.Success)
                        {
                            messageModel.Success = false;
                        }
                        messageModel.Msg += info.Msg;
                    }
                }

                return messageModel;
            }
        }

        public async Task<MessageModel<WeChatApiDto>> PushText(string token, WeChatPushTestDto msg)
        {

            object data = null; ;
            WeChatApiDto pushres = null; ;
            //订阅用户  
            switch (msg.SelectMsgType)
            {
                case "text":
                    //发送文本 
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.SelectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.SelectUser,
                        msgtype = msg.SelectMsgType,
                        text = new
                        {
                            content = msg.TextContent.Text
                        }
                    };

                    if (msg.SelectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                case "image":
                    //发送图片 
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.SelectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.SelectUser,
                        msgtype = msg.SelectMsgType,
                        images = new
                        {
                            media_ids = new List<string> {
                                msg.PictureContent.PictureMediaID
                            },
                            recommend = "xxx",
                            need_open_comment = 1,
                            only_fans_can_comment = 0
                        }
                    };
                    if (msg.SelectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                case "voice":
                    //发送音频
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.SelectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.SelectUser,
                        msgtype = msg.SelectMsgType,
                        voice = new
                        {
                            media_id = msg.VoiceContent.VoiceMediaID
                        }
                    };
                    if (msg.SelectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                case "mpvideo":
                    //发送视频
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.SelectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.SelectUser,
                        msgtype = msg.SelectMsgType,
                        mpvideo = new
                        {
                            media_id = msg.VideoContent.VideoMediaID,
                        }
                    };
                    if (msg.SelectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                default:
                    pushres = new WeChatApiDto() { Errcode = -1, Errmsg = $"未找到推送类型{msg.SelectMsgType}" };
                    break;
            }
            if (pushres.Errcode.Equals(0))
            {
                return MessageModel<WeChatApiDto>.SUCCESS("推送成功", pushres);

            }
            else
            {
                return MessageModel<WeChatApiDto>.FAIL($"\r\n推送失败\r\n错误代码:{pushres.Errcode}\r\n错误信息:{pushres.Errmsg}", pushres);
            }
        }

        public async Task<MessageModel<WeChatApiDto>> UpdateMenu(WeChatApiDto menu)
        {
            WeChatHelper.ConverMenuButtonForEvent(menu);
            var res = await GetToken(menu.Id);
            if (!res.Success)
                return res;
            var data = await WeChatHelper.SetMenu(res.Response.Access_Token, JsonHelper.ObjToJson(menu.Menu));
            if (data.Errcode.Equals(0))
            {

                return MessageModel<WeChatApiDto>.SUCCESS("更新成功", data);
            }
            else
            {
                return MessageModel<WeChatApiDto>.SUCCESS("更新失败", data);
            }
        }

        public async Task<MessageModel<WeChatResponseUserInfo>> GetBindUserInfo(WeChatUserInfo info)
        {
            var bindUser = await base.BaseDal.Db.Queryable<WeChatSub>().Where(it => it.SubFromPublicAccount == info.Id && it.CompanyId == info.CompanyCode && info.UserID.Equals(it.SubJobId) && it.IsUnBind == false)
                .FirstAsync();
            if (bindUser == null)
                return MessageModel<WeChatResponseUserInfo>.FAIL("用户不存在或者已经解绑!");
            var res = await GetToken(info.Id);
            if (!res.Success)
                return MessageModel<WeChatResponseUserInfo>.FAIL(res.Msg);
            var token = res.Response.Access_Token;
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo
            {
                CompanyCode = info.CompanyCode,
                Id = info.Id
            };
            var data = await WeChatHelper.GetUserInfo(token, bindUser.SubUserOpenID);
            reData.UsersData = data;
            if (data.Errcode.Equals(0))
            {
                return MessageModel<WeChatResponseUserInfo>.SUCCESS("用户信息获取成功", reData);
            }
            else
            {
                return MessageModel<WeChatResponseUserInfo>.FAIL("用户信息获取失败", reData);
            }
        }

        public async Task<MessageModel<WeChatResponseUserInfo>> UnBind(WeChatUserInfo info)
        {
            var bindUser = await base.BaseDal.Db.Queryable<WeChatSub>()
                .Where(t => t.SubFromPublicAccount == info.Id && t.CompanyId == info.CompanyCode && info.UserID.Equals(t.SubJobId) && t.IsUnBind == false)
                .FirstAsync();
            if (bindUser == null)
                return MessageModel<WeChatResponseUserInfo>.FAIL("用户不存在或者已经解绑!");
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo
            {
                CompanyCode = info.CompanyCode,
                Id = info.Id
            };
            bindUser.IsUnBind = true;
            bindUser.SubUserRefTime = DateTime.Now;
            await BaseDal.Db.Updateable(bindUser)
                .UpdateColumns(t => new { t.IsUnBind, t.SubUserRefTime })
                .ExecuteCommandAsync();
            return MessageModel<WeChatResponseUserInfo>.SUCCESS("用户解绑成功", reData);
        }

        public async Task<string> HandleWeChat(WeChatXMLDto weChat)
        {
            switch (weChat.MsgType)
            {
                case "text":
                    return await HandText(weChat);
                case "image":
                    return await HandImage(weChat);
                case "voice":
                    return await HandVoice(weChat);
                case "shortvideo":
                    return await HandShortvideo(weChat);
                case "location":
                    return await HandLocation(weChat);
                case "link":
                    return await HandLink(weChat);
                case "event":
                    return await HandEvent(weChat);
                default:
                    return await Task.Run(() =>
                    {
                        return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[处理失败,没有找到消息类型=>{weChat.MsgType}]]></Content></xml>";
                    });
            }
        }

        #region 私有方法

        /// <summary>
        /// 处理文本
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandText(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了文本=>{weChat.Content}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 处理图片
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandImage(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了图片=>{weChat.PicUrl}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 处理声音
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandVoice(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了声音=>{weChat.MediaId}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 处理小视频
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandShortvideo(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了小视频=>{weChat.MediaId}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 处理地理位置
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandLocation(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了地址位置=>{weChat.Label}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 处理链接消息
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandLink(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了链接消息=>{weChat.Url}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> HandEvent(WeChatXMLDto weChat)
        {

            switch (weChat.Event)
            {
                case "subscribe":
                    return await EventSubscribe(weChat);
                case "unsubscribe":
                    return await EventUnsubscribe(weChat);
                case "SCAN":
                    return await EventSCAN(weChat);
                case "LOCATION":
                    return await EventLOCATION(weChat);
                case "CLICK":
                    return await EventCLICK(weChat);
                case "VIEW":
                    return await EventVIEW(weChat);
                default:
                    return await Task.Run(() =>
                    {
                        return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[处理失败,没有找到事件类型=>{weChat.Event}]]></Content></xml>";
                    });
            }
        }

        /// <summary>
        /// 关注事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> EventSubscribe(WeChatXMLDto weChat)
        {
            if (weChat.EventKey != null && (weChat.EventKey.Equals("bind") || weChat.EventKey.Equals("qrscene_bind")))
            {
                return await QRBind(weChat);
            }
            else
            {
                return await Task.Run(() =>
                {
                    return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了已关注事件=>key:{weChat.EventKey}=>ticket:{weChat.Ticket}]]></Content></xml>";
                });
            }
        }

        /// <summary>
        /// 取消关注事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> EventUnsubscribe(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了取消关注事件=>{weChat.Event}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 已关注扫码事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> EventSCAN(WeChatXMLDto weChat)
        {
            if (weChat.EventKey != null && (weChat.EventKey.StartsWith("bind_") || weChat.EventKey.StartsWith("qrscene_bind_")))
            {

                return await QRBind(weChat);
            }
            else
            {
                return await Task.Run(() =>
                {
                    return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了已关注扫码事件=>key:{weChat.EventKey}=>ticket:{weChat.Ticket}]]></Content></xml>";
                });

            }

        }

        /// <summary>
        /// 扫码绑定
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> QRBind(WeChatXMLDto weChat)
        {
            var ticket = await base.BaseDal.Db.Queryable<WeChatQR>()
                .InSingleAsync(weChat.Ticket);
            if (ticket == null)
                throw new Exception("ticket未找到");
            if (ticket.IsUsed)
                throw new Exception("ticket已被使用");
            if (!ticket.PublicAccount.Equals(weChat.PublicAccount))
                throw new Exception($"公众号错误  need:{ticket.PublicAccount}  but:{weChat.PublicAccount}");

            var bindUser = await BaseDal.Db.Queryable<WeChatSub>()
                .Where(t => t.SubFromPublicAccount == ticket.PublicAccount && t.CompanyId == ticket.BindCompanyId && t.SubJobId == ticket.BindJobId)
                .SingleAsync();
            bool isNewBind;
            if (bindUser == null)
            {
                isNewBind = true;
                bindUser = new WeChatSub
                {
                    SubFromPublicAccount = ticket.PublicAccount,
                    CompanyId = ticket.BindCompanyId,
                    SubJobId = ticket.BindJobId,
                    SubUserOpenID = weChat.FromUserName,
                    SubUserRegTime = DateTime.Now
                };
            }
            else
            {
                isNewBind = false;
                //订阅过的就更新
                if (bindUser.SubUserOpenID != weChat.FromUserName)
                    //记录上一次的订阅此工号的微信号
                    bindUser.LastSubUserOpenID = bindUser.SubUserOpenID;
                bindUser.SubUserOpenID = weChat.FromUserName;
                bindUser.SubUserRefTime = DateTime.Now;
                bindUser.IsUnBind = false;
            }
            ticket.IsUsed = true;
            ticket.UseTime = DateTime.Now;
            ticket.UseOpenid = weChat.FromUserName;

            try
            {
                _unitOfWorkManage.BeginTran();
                await base.BaseDal.Db.Updateable(ticket)
                    .ExecuteCommandAsync();
                if (isNewBind)
                    await base.BaseDal.Db.Insertable(bindUser)
                        .ExecuteCommandAsync();
                else
                    await base.BaseDal.Db.Updateable(bindUser)
                        .ExecuteCommandAsync();
                _unitOfWorkManage.CommitTran();
            }
            catch
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }

            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[恭喜您:{(string.IsNullOrEmpty(ticket.BindJobNick) ? ticket.BindJobId : ticket.BindJobNick)},绑定成功!]]></Content></xml>";
        }

        /// <summary>
        /// 上报位置地理事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> EventLOCATION(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了地理位置事件=>维度:{weChat.Latitude}经度:{weChat.Longitude}位置精度:{weChat.Precision}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 点击菜单按钮事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> EventCLICK(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了菜单点击按钮事件=>{weChat.EventKey}]]></Content></xml>";
            });
        }

        /// <summary>
        /// 点击菜单网址事件
        /// </summary>
        /// <param name="weChat"></param>
        /// <returns></returns>
        private async Task<string> EventVIEW(WeChatXMLDto weChat)
        {
            return await Task.Run(() =>
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了菜单点击网址事件=>{weChat.EventKey}]]></Content></xml>";
            });
        }

        #endregion
    }
}
