using WinFormsExam.Domain;

namespace WinFormsExam.Data;

public interface IDeliveryRepository
{
    int Insert(Delivery delivery);

    IReadOnlyList<Delivery> LoadAll();

    void UpdateStatus(int id, DeliveryStatus status);
}
