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
using repo_nha_hang_com_ga_BE.Models.Requests.TuDo;
using repo_nha_hang_com_ga_BE.Models.Responds.TuDo;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class TuDoRepository : ITuDoRepository
{
    private readonly IMongoCollection<TuDo> _collection;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<LoaiTuDo> _collectionLoaiTuDo;
    public TuDoRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<TuDo>("TuDo");
        _collectionLoaiTuDo = database.GetCollection<LoaiTuDo>("LoaiTuDo");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<TuDoRespond>>> GetAllTuDos(RequestSearchTuDo request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<TuDo>.Filter.Empty;
            filter &= Builders<TuDo>.Filter.Eq(x => x.isDelete, false);


            if (!string.IsNullOrEmpty(request.tenTuDo))
            {
                filter &= Builders<TuDo>.Filter.Regex(x => x.tenTuDo, new BsonRegularExpression($".*{request.tenTuDo}.*"));
            }

            if (!string.IsNullOrEmpty(request.loaiTuDoId))
            {
                filter &= Builders<TuDo>.Filter.Eq(x => x.loaiTuDo, request.loaiTuDoId);
            }

            var projection = Builders<TuDo>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenTuDo)
                .Include(x => x.moTa)
                .Include(x => x.loaiTuDo);

            var findOptions = new FindOptions<TuDo, TuDo>
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
                var tuDos = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiTuDoIds = tuDos.Select(x => x.loaiTuDo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiTuDoFilter = Builders<LoaiTuDo>.Filter.In(x => x.Id, loaiTuDoIds);
                var loaiTuDoProjection = Builders<LoaiTuDo>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiTuDos = await _collectionLoaiTuDo.Find(loaiTuDoFilter)
                    .Project<LoaiTuDo>(loaiTuDoProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiTuDoDict = loaiTuDos.ToDictionary(x => x.Id, x => x.tenLoai);

                // Map dữ liệu
                var tuDoResponds = tuDos.Select(tuDo => new TuDoRespond
                {
                    id = tuDo.Id,
                    tenTuDo = tuDo.tenTuDo,
                    moTa = tuDo.moTa,
                    loaiTuDo = new IdName
                    {
                        Id = tuDo.loaiTuDo,
                        Name = tuDo.loaiTuDo != null && loaiTuDoDict.ContainsKey(tuDo.loaiTuDo) ? loaiTuDoDict[tuDo.loaiTuDo] : null
                    }
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<TuDoRespond>>
                {
                    Paging = pagingDetail,
                    Data = tuDoResponds
                };

                return new RespondAPIPaging<List<TuDoRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var tuDos = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiTuDoIds = tuDos.Select(x => x.loaiTuDo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiTuDoFilter = Builders<LoaiTuDo>.Filter.In(x => x.Id, loaiTuDoIds);
                var loaiTuDoProjection = Builders<LoaiTuDo>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiTuDos = await _collectionLoaiTuDo.Find(loaiTuDoFilter)
                    .Project<LoaiTuDo>(loaiTuDoProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiTuDoDict = loaiTuDos.ToDictionary(x => x.Id, x => x.tenLoai);

                // Map dữ liệu
                var tuDoResponds = tuDos.Select(tuDo => new TuDoRespond
                {
                    id = tuDo.Id,
                    tenTuDo = tuDo.tenTuDo,
                    moTa = tuDo.moTa,
                    loaiTuDo = new IdName
                    {
                        Id = tuDo.loaiTuDo,
                        Name = tuDo.loaiTuDo != null && loaiTuDoDict.ContainsKey(tuDo.loaiTuDo) ? loaiTuDoDict[tuDo.loaiTuDo] : null
                    }
                }).ToList();

                return new RespondAPIPaging<List<TuDoRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<TuDoRespond>>
                    {
                        Data = tuDoResponds,
                        Paging = new PagingDetail(1, tuDoResponds.Count, tuDoResponds.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<TuDoRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<TuDoRespond>> GetTuDoById(string id)
    {
        try
        {
            var tuDo = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (tuDo == null)
            {
                return new RespondAPI<TuDoRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy tủ đồ với ID đã cung cấp."
                );
            }

            var loaiTuDo = await _collectionLoaiTuDo.Find(x => x.Id == tuDo.loaiTuDo).FirstOrDefaultAsync();
            var tuDoRespond = _mapper.Map<TuDoRespond>(tuDo);
            tuDoRespond.loaiTuDo = new IdName
            {
                Id = loaiTuDo.Id,
                Name = loaiTuDo.tenLoai
            };

            return new RespondAPI<TuDoRespond>(
                ResultRespond.Succeeded,
                "Lấy tủ đồ thành công.",
                tuDoRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<TuDoRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<TuDoRespond>> CreateTuDo(RequestAddTuDo request)
    {
        try
        {
            TuDo newTuDo = _mapper.Map<TuDo>(request);

            newTuDo.createdDate = DateTimeOffset.UtcNow;
            newTuDo.updatedDate = DateTimeOffset.UtcNow;
            newTuDo.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newLoaiNguyenLieu.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newTuDo);

            var loaiTuDo = await _collectionLoaiTuDo.Find(x => x.Id == newTuDo.loaiTuDo).FirstOrDefaultAsync();
            var tuDoRespond = _mapper.Map<TuDoRespond>(newTuDo);
            tuDoRespond.loaiTuDo = new IdName
            {
                Id = loaiTuDo.Id,
                Name = loaiTuDo.tenLoai
            };

            return new RespondAPI<TuDoRespond>(
                ResultRespond.Succeeded,
                "Tạo tủ đồ thành công.",
                tuDoRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<TuDoRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo tủ đồ: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<TuDoRespond>> UpdateTuDo(string id, RequestUpdateTuDo request)
    {
        try
        {
            var filter = Builders<TuDo>.Filter.Eq(x => x.Id, id);
            filter &= Builders<TuDo>.Filter.Eq(x => x.isDelete, false);
            var tuDo = await _collection.Find(filter).FirstOrDefaultAsync();

            if (tuDo == null)
            {
                return new RespondAPI<TuDoRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy tủ đồ với ID đã cung cấp."
                );
            }

            _mapper.Map(request, tuDo);

            tuDo.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, tuDo);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<TuDoRespond>(
                    ResultRespond.Error,
                    "Cập nhật tủ đồ không thành công."
                );
            }

            var loaiTuDo = await _collectionLoaiTuDo.Find(x => x.Id == tuDo.loaiTuDo).FirstOrDefaultAsync();
            var tuDoRespond = _mapper.Map<TuDoRespond>(tuDo);
            tuDoRespond.loaiTuDo = new IdName
            {
                Id = loaiTuDo.Id,
                Name = loaiTuDo.tenLoai
            };

            return new RespondAPI<TuDoRespond>(
                ResultRespond.Succeeded,
                "Cập nhật tủ đồ thành công.",
                tuDoRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<TuDoRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật tủ đồ: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteTuDo(string id)
    {
        try
        {
            var existingTuDo = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingTuDo == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy tủ đồ để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa tủ đồ không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa tủ đồ thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa tủ đồ: {ex.Message}"
            );
        }
    }
}