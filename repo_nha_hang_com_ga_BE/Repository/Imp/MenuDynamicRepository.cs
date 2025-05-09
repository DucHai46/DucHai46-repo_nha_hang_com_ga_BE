using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
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

            if (!string.IsNullOrEmpty(request.parentId))
            {
                filter &= Builders<MenuDynamic>.Filter.Eq(x => x.parent, request.parentId);
            }

            if (request.isActive != null)
            {
                filter &= Builders<MenuDynamic>.Filter.Eq(x => x.isActive, request.isActive);
            }


            var projection = Builders<MenuDynamic>.Projection
                .Include(x => x.Id)
                .Include(x => x.routeLink)
                .Include(x => x.icon)
                .Include(x => x.label)
                .Include(x => x.isOpen)
                .Include(x => x.parent)
                .Include(x => x.position)
                .Include(x => x.isActive);
            // .Include(x => x.Children);


            var findOptions = new FindOptions<MenuDynamic, MenuDynamic>
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

                // Lấy danh sách ID loại bàn
                var parentIds = menuDynamics.Select(x => x.parent).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var parentFilter = Builders<MenuDynamic>.Filter.In(x => x.Id, parentIds);
                var parentProjection = Builders<MenuDynamic>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.label);
                var parents = await _collection.Find(parentFilter)
                    .Project<MenuDynamic>(parentProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var parentDict = parents.ToDictionary(x => x.Id, x => x.label);

                // Map dữ liệu
                var menuDynamicResponds = menuDynamics.Select(menuDynamic => new MenuDynamicRespond
                {
                    id = menuDynamic.Id,
                    label = menuDynamic.label,
                    routeLink = menuDynamic.routeLink,
                    icon = menuDynamic.icon,
                    parent = new IdName
                    {
                        Id = menuDynamic.parent,
                        Name = menuDynamic.parent != null && parentDict.ContainsKey(menuDynamic.parent) ? parentDict[menuDynamic.parent] : null
                    },
                    position = menuDynamic.position,
                    isActive = menuDynamic.isActive
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<MenuDynamicRespond>>
                {
                    Paging = pagingDetail,
                    Data = menuDynamicResponds
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

                // Lấy danh sách ID loại bàn
                var parentIds = menuDynamics.Select(x => x.parent).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var parentFilter = Builders<MenuDynamic>.Filter.In(x => x.Id, parentIds);
                var parentProjection = Builders<MenuDynamic>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.label);
                var parents = await _collection.Find(parentFilter)
                    .Project<MenuDynamic>(parentProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var parentDict = parents.ToDictionary(x => x.Id, x => x.label);

                // Map dữ liệu
                var menuDynamicResponds = menuDynamics.Select(menuDynamic => new MenuDynamicRespond
                {
                    id = menuDynamic.Id,
                    label = menuDynamic.label,
                    routeLink = menuDynamic.routeLink,
                    icon = menuDynamic.icon,
                    parent = new IdName
                    {
                        Id = menuDynamic.parent,
                        Name = menuDynamic.parent != null && parentDict.ContainsKey(menuDynamic.parent) ? parentDict[menuDynamic.parent] : null
                    },
                    position = menuDynamic.position,
                    isActive = menuDynamic.isActive
                }).ToList();

                return new RespondAPIPaging<List<MenuDynamicRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<MenuDynamicRespond>>
                    {
                        Data = menuDynamicResponds,
                        Paging = new PagingDetail(1, menuDynamicResponds.Count, menuDynamicResponds.Count)
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

            var parent = await _collection.Find(x => x.Id == menuDynamic.parent).FirstOrDefaultAsync();
            var menuDynamicRespond = new MenuDynamicRespond
            {
                id = menuDynamic.Id,
                label = menuDynamic.label,
                routeLink = menuDynamic.routeLink,
                icon = menuDynamic.icon,
                parent = new IdName
                {
                    Id = menuDynamic.parent,
                    Name = parent != null ? parent.label : null
                },
                position = menuDynamic.position,
                isActive = menuDynamic.isActive
            };

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

            var parent = await _collection.Find(x => x.Id == newMenuDynamic.parent).FirstOrDefaultAsync();
            var menuDynamicRespond = new MenuDynamicRespond
            {
                id = newMenuDynamic.Id,
                label = newMenuDynamic.label,
                routeLink = newMenuDynamic.routeLink,
                icon = newMenuDynamic.icon,
                parent = new IdName
                {
                    Id = newMenuDynamic.parent,
                    Name = parent != null ? parent.label : null
                },
                position = newMenuDynamic.position,
                isActive = newMenuDynamic.isActive
            };
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

            var parent = await _collection.Find(x => x.Id == menuDynamic.parent).FirstOrDefaultAsync();
            var menuDynamicRespond = new MenuDynamicRespond
            {
                id = menuDynamic.Id,
                label = menuDynamic.label,
                routeLink = menuDynamic.routeLink,
                icon = menuDynamic.icon,
                parent = new IdName
                {
                    Id = menuDynamic.parent,
                    Name = parent != null ? parent.label : null
                },
                position = menuDynamic.position,
                isActive = menuDynamic.isActive
            };

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