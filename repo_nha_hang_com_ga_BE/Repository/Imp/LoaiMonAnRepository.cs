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
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiMonAn;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiMonAn;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class LoaiMonAnRepository : ILoaiMonAnRepository
{
    private readonly IMongoCollection<LoaiMonAn> _collection;
    private readonly IMongoCollection<DanhMucMonAn> _collectionDanhMucMonAn;
    private readonly IMapper _mapper;

    public LoaiMonAnRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<LoaiMonAn>("LoaiMonAn");
        _collectionDanhMucMonAn = database.GetCollection<DanhMucMonAn>("DanhMucMonAn");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<LoaiMonAnRespond>>> GetAllLoaiMonAns(RequestSearchLoaiMonAn request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<LoaiMonAn>.Filter.Empty;
            filter &= Builders<LoaiMonAn>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.danhMucMonAnId))
            {
                filter &= Builders<LoaiMonAn>.Filter.Eq(x => x.danhMucMonAn, request.danhMucMonAnId);
            }

            if (!string.IsNullOrEmpty(request.tenLoai))
            {
                filter &= Builders<LoaiMonAn>.Filter.Regex(x => x.tenLoai, new BsonRegularExpression($".*{request.tenLoai}.*"));
            }

            var projection = Builders<LoaiMonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai)
                .Include(x => x.danhMucMonAn)
                .Include(x => x.moTa);

            var findOptions = new FindOptions<LoaiMonAn, LoaiMonAn>
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
                var loaiMonAns = await cursor.ToListAsync();

                var danhMucMonAnIds = loaiMonAns.Select(x => x.danhMucMonAn).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var danhMucMonAnFilter = Builders<DanhMucMonAn>.Filter.In(x => x.Id, danhMucMonAnIds);
                var danhMucMonAnProjection = Builders<DanhMucMonAn>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenDanhMuc);
                var danhMucMonAns = await _collectionDanhMucMonAn.Find(danhMucMonAnFilter)
                    .Project<DanhMucMonAn>(danhMucMonAnProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var danhMucMonAnDict = danhMucMonAns.ToDictionary(x => x.Id, x => x.tenDanhMuc);

                // Map dữ liệu
                var loaiMonAnResponds = loaiMonAns.Select(loaiMonAn => new LoaiMonAnRespond
                {
                    id = loaiMonAn.Id,
                    tenLoai = loaiMonAn.tenLoai,
                    moTa = loaiMonAn.moTa,
                    danhMucMonAn = new IdName
                    {
                        Id = loaiMonAn.danhMucMonAn,
                        Name = loaiMonAn.danhMucMonAn != null && danhMucMonAnDict.ContainsKey(loaiMonAn.danhMucMonAn) ? danhMucMonAnDict[loaiMonAn.danhMucMonAn] : null
                    }
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<LoaiMonAnRespond>>
                {
                    Paging = pagingDetail,
                    Data = loaiMonAnResponds
                };

                return new RespondAPIPaging<List<LoaiMonAnRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var loaiMonAns = await cursor.ToListAsync();

                var danhMucMonAnIds = loaiMonAns.Select(x => x.danhMucMonAn).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var danhMucMonAnFilter = Builders<DanhMucMonAn>.Filter.In(x => x.Id, danhMucMonAnIds);
                var danhMucMonAnProjection = Builders<DanhMucMonAn>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenDanhMuc);
                var danhMucMonAns = await _collectionDanhMucMonAn.Find(danhMucMonAnFilter)
                    .Project<DanhMucMonAn>(danhMucMonAnProjection)
                    .ToListAsync();

                var danhMucMonAnDict = danhMucMonAns.ToDictionary(x => x.Id, x => x.tenDanhMuc);

                var loaiMonAnResponds = loaiMonAns.Select(loaiMonAn => new LoaiMonAnRespond
                {
                    id = loaiMonAn.Id,
                    tenLoai = loaiMonAn.tenLoai,
                    moTa = loaiMonAn.moTa,
                    danhMucMonAn = new IdName
                    {
                        Id = loaiMonAn.danhMucMonAn,
                        Name = loaiMonAn.danhMucMonAn != null && danhMucMonAnDict.ContainsKey(loaiMonAn.danhMucMonAn) ? danhMucMonAnDict[loaiMonAn.danhMucMonAn] : null
                    }
                }).ToList();

                return new RespondAPIPaging<List<LoaiMonAnRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<LoaiMonAnRespond>>
                    {
                        Data = loaiMonAnResponds,
                        Paging = new PagingDetail(1, loaiMonAnResponds.Count, loaiMonAnResponds.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<LoaiMonAnRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<LoaiMonAnRespond>> GetLoaiMonAnById(string id)
    {
        try
        {
            var loaiMonAn = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();


            if (loaiMonAn == null)
            {
                return new RespondAPI<LoaiMonAnRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại món ăn với ID đã cung cấp."
                );
            }

            var loaiMonAnRespond = _mapper.Map<LoaiMonAnRespond>(loaiMonAn);
            var danhMucMonAn = await _collectionDanhMucMonAn.Find(x => x.Id == loaiMonAn.danhMucMonAn).FirstOrDefaultAsync();
            loaiMonAnRespond.danhMucMonAn = new IdName
            {
                Id = danhMucMonAn.Id,
                Name = danhMucMonAn.tenDanhMuc
            };

            return new RespondAPI<LoaiMonAnRespond>(
                ResultRespond.Succeeded,
                "Lấy loại món ăn thành công.",
                loaiMonAnRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiMonAnRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<LoaiMonAnRespond>> CreateLoaiMonAn(RequestAddLoaiMonAn request)
    {
        try
        {
            LoaiMonAn newLoaiMonAn = _mapper.Map<LoaiMonAn>(request);

            newLoaiMonAn.createdDate = DateTimeOffset.UtcNow;
            newLoaiMonAn.updatedDate = DateTimeOffset.UtcNow;
            newLoaiMonAn.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newLoaiNguyenLieu.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newLoaiMonAn);

            var loaiMonAnRespond = _mapper.Map<LoaiMonAnRespond>(newLoaiMonAn);
            var danhMucMonAn = await _collectionDanhMucMonAn.Find(x => x.Id == newLoaiMonAn.danhMucMonAn).FirstOrDefaultAsync();
            loaiMonAnRespond.danhMucMonAn = new IdName
            {
                Id = danhMucMonAn.Id,
                Name = danhMucMonAn.tenDanhMuc
            };

            return new RespondAPI<LoaiMonAnRespond>(
                ResultRespond.Succeeded,
                "Tạo loại món ăn thành công.",
                loaiMonAnRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiMonAnRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo loại món ăn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<LoaiMonAnRespond>> UpdateLoaiMonAn(string id, RequestUpdateLoaiMonAn request)
    {
        try
        {
            var filter = Builders<LoaiMonAn>.Filter.Eq(x => x.Id, id);
            filter &= Builders<LoaiMonAn>.Filter.Eq(x => x.isDelete, false);
            var loaiMonAn = await _collection.Find(filter).FirstOrDefaultAsync();

            if (loaiMonAn == null)
            {
                return new RespondAPI<LoaiMonAnRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại món ăn với ID đã cung cấp."
                );
            }

            _mapper.Map(request, loaiMonAn);

            loaiMonAn.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, loaiMonAn);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<LoaiMonAnRespond>(
                    ResultRespond.Error,
                    "Cập nhật loại món ăn không thành công."
                );
            }

            var loaiMonAnRespond = _mapper.Map<LoaiMonAnRespond>(loaiMonAn);
            var danhMucMonAn = await _collectionDanhMucMonAn.Find(x => x.Id == loaiMonAn.danhMucMonAn).FirstOrDefaultAsync();
            loaiMonAnRespond.danhMucMonAn = new IdName
            {
                Id = danhMucMonAn.Id,
                Name = danhMucMonAn.tenDanhMuc
            };

            return new RespondAPI<LoaiMonAnRespond>(
                ResultRespond.Succeeded,
                "Cập nhật loại món ăn thành công.",
                loaiMonAnRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiMonAnRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật loại món ăn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteLoaiMonAn(string id)
    {
        try
        {
            var existingLoaiMonAn = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingLoaiMonAn == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại món ăn để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa loại món ăn không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa loại món ăn thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa loại món ăn: {ex.Message}"
            );
        }
    }
}