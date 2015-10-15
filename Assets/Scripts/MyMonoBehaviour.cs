using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// MonoBehaviourに独自の機能を追加する
/// </summary>
public class MyMonoBehaviour : MonoBehaviour
{
    private Player _player = null; // playerプロパティの実態

    /// <summary>
    /// Playerを取得
    /// </summary>
    public Player player
    {
        get
        {
            if(_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(Tags.Player);
                if (go != null)
                {
                    _player = go.GetComponentInParent<Player>();
                }
            }
            return _player;
        }
    }

    private Renderer _renderer;	// rendererプロパティの実態

    /// <summary>
    /// 自分自身のRendererを取得する
    /// </summary>
    public new virtual Renderer renderer
    {
        get
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            return _renderer;
        }
    }

    private Collider _collider;	// colliderプロパティの実態

    /// <summary>
    /// 自分自身のcollideを取得する
    /// </summary>
    public new virtual Collider collider
    {
        get
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }

            return _collider;
        }
    }

    private Rigidbody _rigidbody;	// rigidbodyプロパティの実態

    /// <summary>
    /// 自分自身のrigidbodyを取得する
    /// </summary>
    public new Rigidbody rigidbody
    {
        get
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            return _rigidbody;
        }
    }

    /// <summary>
    /// 表示状態
    /// </summary>
    public virtual bool IsShow
    {
		// 子も含めて初めに見つけたRendererの状態を返す
        get
        {
			foreach(Renderer renderer in GetComponentsInChildren<Renderer>())
			{
				return renderer.enabled;
			}

			return false;
        }

		// 子も含めて全てのRendererの表示状態を変更する
		set
		{
			foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
			{
				renderer.enabled = value;
			}

			foreach (Light renderer in GetComponentsInChildren<Light>())
			{
				renderer.enabled = value;
			}
        }
    }

    protected virtual void Awake()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void LateUpdate()
    {
    }

    public T GetSafeComponent<T>() where T : MonoBehaviour
    {
        T component = GetComponent<T>();

        if (component == null)
        {
            Debug.LogError("Expected to find component of type "
               + typeof(T) + " but found none", this);
        }

        return component;
    }
}
