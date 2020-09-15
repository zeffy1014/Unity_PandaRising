using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DataBase;
using Zenject;

namespace Bullet
{
    public enum BulletType
    {
        Player_Mikan,
        Player_Kaju,
        Player_Fish,
        Player_Block,
        Enemy_Circle_Straight,
        Enemy_Circle_Homing,
        Enemy_Needle_Street,

        BulletType_Num
    }

    public class BulletGenerator : ILoadData
    {
        GameObject[] bulletPrefabs = new GameObject[(int)BulletType.BulletType_Num];

        // Zenject Signal送信用(生成したBullet側に必要に応じて渡す)
        SignalBus signalBus;

        /***** 読み込み完了監視 ************************************************************/
        ReactiveProperty<bool> _onLoadCompleteProperty = new ReactiveProperty<bool>(false);
        public IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty => _onLoadCompleteProperty;
        public bool LoadCompleted() { return _onLoadCompleteProperty.Value; }

        BulletGenerator(SignalBus sb)
        {
            signalBus = sb;

            bool loadResult = false;
            bool loadPrefabNull = false;

            // 一通りのBulletPrefabを読み込みしておく
            for (int index=0; index < (int)BulletType.BulletType_Num; index++)
            {
                // Prefab読み込んでnullだったら読み込み失敗となる
                if (!GetBulletPrefab((BulletType)index))
                {
                    loadPrefabNull = true;
                    break;
                }
            }

            // 強化レベルによる威力など反映しなおす
            Bullet_Player_Mikan.SetPower();
            Bullet_Player_Fish.SetPower();

            // Signalを渡すものに渡す
            Bullet_Player_Fish.SetSignalBus(signalBus);

            // 読み込み成否確認
            if (!loadPrefabNull) loadResult = true;

            if (true == loadResult)
            {
                // 読み込み完了したらフラグを立てる
                _onLoadCompleteProperty.Value = true;
            }
            else
            {
                // TODO:読み込み失敗したらエラー通知してメインメニューに戻る？
                Debug.Log("BulletGenerator load data failed...");
            }
        }

        // Bullet Prefabを取得(未取得ならLoad)
        GameObject GetBulletPrefab(BulletType type)
        {
            if (null == bulletPrefabs[(int)type])
            {
                // DataLibrarianからパス取得してロードする
                bulletPrefabs[(int)type] = (GameObject)Resources.Load(DataLibrarian.Instance.GetBulletPrefabPath(type));
            }

            return bulletPrefabs[(int)type];
        }

        // 弾を発射(単発)  種別と生成位置と角度は必須　通常はこちらを使えばよい
        public void ShotBullet(
            Vector2 genPos,
            Vector2 genRot,
            BulletType type,
            float angle)
        {
            // Prefab取得して生成
            GameObject bullet = Object.Instantiate<GameObject>(GetBulletPrefab(type), genPos, Quaternion.Euler(genRot));

            // 角度指定して発射
            bullet.GetComponent<Bullet>().Shot(angle);

        }

        // 弾を発射(単発・デフォルトから変えたいパラメータあり)
        public void ShotBullet(
            Vector2 genPos, 
            Vector2 genRot, 
            BulletType type, 
            float angle, 
            float? moveSpeed = null, float? rotateSpeed = null, float? accel = null, float? size = null, Color? color = null) // default引数はNullable<T>を使ってnull扱いとする
        {
            // Prefab取得して生成
            GameObject bullet = Object.Instantiate<GameObject>(GetBulletPrefab(type), genPos, Quaternion.Euler(genRot));

            // パラメータ上書きがあれば行う
            bullet.GetComponent<Bullet>().SetParam(moveSpeed, rotateSpeed, accel, size, color);

            // 角度指定して発射
            bullet.GetComponent<Bullet>().Shot(angle);

        }
    }
}