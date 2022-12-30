using Astrarium.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public interface IDatabaseManager
    {
        Task<Observation> CreateObservation(Session session, CelestialObject body, TargetDetails targetDetails, DateTime begin, DateTime end);
        Task DeleteCamera(string id);
        Task DeleteEyepiece(string id);
        Task DeleteFilter(string id);
        Task DeleteLens(string id);
        Task DeleteObservation(string id);

        Task DeleteSession(string id);

        Task DeleteOptics(string id);
        Task EditObservation(Observation observation, CelestialObject body, DateTime begin, DateTime end);
        Task<Camera> GetCamera(string id);
        Task<ICollection> GetCameras();
        Task<Eyepiece> GetEyepiece(string id);
        Task<ICollection> GetEyepieces();
        Task<Filter> GetFilter(string id);
        Task<ICollection> GetFilters();
        Task<Lens> GetLens(string id);
        Task<ICollection> GetLenses();
        Task<ICollection> GetOptics();
        Task<Optics> GetOptics(string id);
        Task<List<Session>> GetSessions();
        Task<ICollection> GetSites();
        Task<CelestialObject> GetTarget(string id);
        Task LoadObservation(Observation observation);
        Task LoadSession(Session session);
        Task<ICollection<string>> GetObservationFiles(string observationId);
        Task<ICollection<string>> GetSessionFiles(string sessionId);
        Task SaveAttachment(Attachment attachment);
        Task SaveCamera(Camera camera);
        void SaveDatabaseEntityProperty(object value, Type entityType, string column, object key);
        Task SaveEyepiece(Eyepiece eyepiece);
        Task SaveFilter(Filter filter);
        Task SaveLens(Lens lens);
        Task SaveOptics(Optics optics);
    }
}