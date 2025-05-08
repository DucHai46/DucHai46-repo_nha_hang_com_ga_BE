
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.DonOrder;
using repo_nha_hang_com_ga_BE.Models.Responds.DonOrder;


namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class DonOrderRepository : IDonOrderRepository
{
    private readonly IMongoCollection<DonOrder> _collection;
    private readonly IMapper _mapper;

    public DonOrderRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<DonOrder>("DonOrder");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<DonOrderRespond>>> GetAllDonOrder(RequestSearchDonOrder request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<DonOrder>.Filter.Empty;
            filter &= Builders<DonOrder>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenDon))
            {
                filter &= Builders<DonOrder>.Filter.Regex(x => x.tenDon, request.tenDon);
            }

            if (!string.IsNullOrEmpty(request.loaiDon))
            {
                filter &= Builders<DonOrder>.Filter.Regex(x => x.loaiDon, request.loaiDon);
                // filter &= Builders<DonOrder>.Filter.Regex(x => x.khachHang!.Name, new BsonRegularExpression($".*{request.khachHangName}.*"));
            }

            if (!string.IsNullOrEmpty(request.banId))
            {
                filter &= Builders<DonOrder>.Filter.Eq(x => x.ban!.Id, request.banId);
            }

            if (request.trangThai.HasValue) // Kiểm tra nếu trangThai có giá trị: True hoặc False
            {
                filter &= Builders<DonOrder>.Filter.Eq(x => x.trangThai, request.trangThai);
            }

            // if (request.ban != null)
            // {
            //     filter &= Builders<DonOrder>.Filter.Eq(x => x.ban, request.ban);
            // }

            var projection = Builders<DonOrder>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenDon)
                .Include(x => x.loaiDon)
                .Include(x => x.ban)
                .Include(x => x.trangThai)
                .Include(x => x.chiTietDonOrder)
                .Include(x => x.tongTien);

            var findOptions = new FindOptions<DonOrder, DonOrderRespond>
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
                var DonOrders = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<DonOrderRespond>>
                {
                    Paging = pagingDetail,
                    Data = DonOrders
                };

                return new RespondAPIPaging<List<DonOrderRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var DonOrders = await cursor.ToListAsync();

                return new RespondAPIPaging<List<DonOrderRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<DonOrderRespond>>
                    {
                        Data = DonOrders,
                        Paging = new PagingDetail(1, DonOrders.Count, DonOrders.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<DonOrderRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<DonOrderRespond>> GetDonOrderById(string id)
    {
        try
        {
            var donOrder = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (donOrder == null)
            {
                return new RespondAPI<DonOrderRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn order với ID đã cung cấp."
                );
            }

            var donOrderRespond = _mapper.Map<DonOrderRespond>(donOrder);

            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Succeeded,
                "Lấy đơn order thành công.",
                donOrderRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<DonOrderRespond>> CreateDonOrder(RequestAddDonOrder request)
    {
        try
        {
            DonOrder newDonOrder = _mapper.Map<DonOrder>(request);

            newDonOrder.createdDate = DateTimeOffset.UtcNow;
            newDonOrder.updatedDate = DateTimeOffset.UtcNow;
            newDonOrder.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newDonOrder);

            var donOrderRespond = _mapper.Map<DonOrderRespond>(newDonOrder);

            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Succeeded,
                "Tạo đơn order thành công.",
                donOrderRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo đơn order: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<DonOrderRespond>> UpdateDonOrder(string id, RequestUpdateDonOrder request)
    {
        try
        {
            var filter = Builders<DonOrder>.Filter.Eq(x => x.Id, id);
            filter &= Builders<DonOrder>.Filter.Eq(x => x.isDelete, false);
            var donOrder = await _collection.Find(filter).FirstOrDefaultAsync();

            if (donOrder == null)
            {
                return new RespondAPI<DonOrderRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn order với ID đã cung cấp."
                );
            }

            _mapper.Map(request, donOrder);

            donOrder.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, donOrder);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<DonOrderRespond>(
                    ResultRespond.Error,
                    "Cập nhật đơn order không thành công."
                );
            }

            var donOrderRespond = _mapper.Map<DonOrderRespond>(donOrder);

            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Succeeded,
                "Cập nhật đơn order thành công.",
                donOrderRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật đơn order: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteDonOrder(string id)
    {
        try
        {
            var existingDonOrder = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingDonOrder == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn order để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa đơn order không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa đơn order thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa đơn order: {ex.Message}"
            );
        }
    }
}