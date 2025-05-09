using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.MenuDynamic;
using repo_nha_hang_com_ga_BE.Models.Responds.MenuDynamic;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class MenuDynamicRepository : IMenuDynamicRepository
{
    private readonly IMongoCollection<MenuDynamic> _collection;
    private readonly IMapper _mapper;

    public MenuDynamicRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<MenuDynamic>("MenuDynamic");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<MenuDynamicRespond>>> GetAllMenuDynamics(RequestSearchMenuDynamic request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<MenuDynamic>.Filter.Empty;
            filter &= Builders<MenuDynamic>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.label))
            {
                filter &= Builders<MenuDynamic>.Filter.Regex(x => x.label, new BsonRegularExpression($".*{request.label}.*"));
            }


            var projection = Builders<MenuDynamic>.Projection
                .Include(x => x.Id)
                .Include(x => x.routeLink)
                .Include(x => x.icon)
                .Include(x => x.label)
                .Include(x => x.isOpen)
                .Include(x => x.children);
            // .Include(x => x.Children);


            var findOptions = new FindOptions<MenuDynamic, MenuDynamicRespond>
            {
                Projection = projection
            };

            if (request.IsPaging)
            {
                long totalRecords = await collection.CountDocumentsAsync(filter);

                int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                int currentPage = request.PageNumber;
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                findOptions.Skip = (currentPage - 1) * request.PageSize;
                findOptions.Limit = request.PageSize;

                var cursor = await collection.FindAsync(filter, findOptions);
                var menuDynamics = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<MenuDynamicRespond>>
                {
                    Paging = pagingDetail,
                    Data = menuDynamics
                };

                return new RespondAPIPaging<List<MenuDynamicRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var menuDynamics = await cursor.ToListAsync();

                return new RespondAPIPaging<List<MenuDynamicRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<MenuDynamicRespond>>
                    {
                        Data = menuDynamics,
                        Paging = new PagingDetail(1, menuDynamics.Count, menuDynamics.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<MenuDynamicRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<MenuDynamicRespond>> GetMenuDynamicById(string id)
    {
        try
        {
            var menuDynamic = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (menuDynamic == null)
            {
                return new RespondAPI<MenuDynamicRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy menu với ID đã cung cấp."
                );
            }

            var menuDynamicRespond = _mapper.Map<MenuDynamicRespond>(menuDynamic);

            return new RespondAPI<MenuDynamicRespond>(
                ResultRespond.Succeeded,
                "Lấy menu thành công.",
                menuDynamicRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<MenuDynamicRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<MenuDynamicRespond>> CreateMenuDynamic(RequestAddMenuDynamic request)
    {
        try
        {
            MenuDynamic newMenuDynamic = _mapper.Map<MenuDynamic>(request);

            newMenuDynamic.createdDate = DateTimeOffset.UtcNow;
            newMenuDynamic.updatedDate = DateTimeOffset.UtcNow;
            newMenuDynamic.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newMenuDynamic);

            var menuDynamicRespond = _mapper.Map<MenuDynamicRespond>(newMenuDynamic);

            return new RespondAPI<MenuDynamicRespond>(
                ResultRespond.Succeeded,
                "Tạo menu thành công.",
                menuDynamicRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<MenuDynamicRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo menu: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<MenuDynamicRespond>> UpdateMenuDynamic(string id, RequestUpdateMenuDynamic request)
    {
        try
        {
            var filter = Builders<MenuDynamic>.Filter.Eq(x => x.Id, id);
            filter &= Builders<MenuDynamic>.Filter.Eq(x => x.isDelete, false);
            var menuDynamic = await _collection.Find(filter).FirstOrDefaultAsync();

            if (menuDynamic == null)
            {
                return new RespondAPI<MenuDynamicRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy menu với ID đã cung cấp."
                );
            }

            _mapper.Map(request, menuDynamic);

            menuDynamic.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, menuDynamic);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<MenuDynamicRespond>(
                    ResultRespond.Error,
                    "Cập nhật menu không thành công."
                );
            }

            var menuDynamicRespond = _mapper.Map<MenuDynamicRespond>(menuDynamic);

            return new RespondAPI<MenuDynamicRespond>(
                ResultRespond.Succeeded,
                "Cập nhật menu thành công.",
                menuDynamicRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<MenuDynamicRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật menu: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteMenuDynamic(string id)
    {
        try
        {
            var existingMenuDynamic = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingMenuDynamic == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy menu để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa menu không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa menu thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa menu: {ex.Message}"
            );
        }
    }

}